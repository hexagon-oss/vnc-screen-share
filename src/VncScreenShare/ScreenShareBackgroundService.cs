using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VncScreenShare
{
	internal class ScreenShareBackgroundService : BackgroundService
	{
		private static readonly TimeSpan ProcessPollInterval = TimeSpan.FromSeconds(3);
		private readonly string m_processName;
		private readonly int m_port;
		private readonly bool m_logFrameRate;
		private readonly ILogger m_logger;
		private readonly Regex m_argument;

		public ScreenShareBackgroundService(string processName, string argumentRegexPattern, int port,
			bool logFrameRate, ILogger<ScreenShareBackgroundService> logger)
		{
			m_processName = processName;
			m_port = port;
			m_logFrameRate = logFrameRate;
			m_logger = logger;
			m_argument = new Regex(argumentRegexPattern);
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(ProcessPollInterval, cancellationToken);
				var process = FindMatchingProcess();
				if (process != null)
				{
					try
					{
						var title = process.MainWindowTitle;
						m_logger.LogInformation($"Start sharing screen of {title}");
						var server = new AppWindowShareServer(process.Id, m_port, m_logFrameRate);
						server.StartSharing();
						m_logger.LogInformation($"Stop sharing screen of {title}");
					}
					catch (Exception e)
					{
						m_logger.LogError(e.ToString());
					}
				}
			}
		}

		private Process FindMatchingProcess()
		{
			foreach (var process in System.Diagnostics.Process.GetProcesses()
				         .Where(x => x.ProcessName == m_processName))
			{
				try
				{
					var commandLine = GetCommandLineArgs(process);
					if (m_argument.IsMatch(commandLine))
					{
						return process;
					}
				}
				catch (Exception e)
				{
					m_logger.LogWarning($"Could not get startup information for process {process.ProcessName} - is service running with required privileges?");
				}
			}
			return null;
		}

		public static string GetCommandLineArgs(Process process)
		{
			try
			{
				using var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
				using var objects = searcher.Get();
				var result = objects.Cast<ManagementBaseObject>().SingleOrDefault();
				return result?["CommandLine"]?.ToString() ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}
	}
}
