using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using ABI.Windows.UI.WindowManagement;
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
		private readonly bool m_moveOffScreen;
		private readonly string m_windowTitle;
		private readonly ILogger m_logger;
		private readonly Regex m_argument;

		public 
			ScreenShareBackgroundService(string processName, string argumentRegexPattern, int port,
			bool logFrameRate, bool moveOffScreen, string windowTitle, ILogger<ScreenShareBackgroundService> logger)
		{
			m_processName = processName;
			m_port = port;
			m_logFrameRate = logFrameRate;
			m_moveOffScreen = moveOffScreen;
			m_windowTitle = windowTitle;
			m_logger = logger;
			m_argument = string.IsNullOrWhiteSpace(argumentRegexPattern)
				? new Regex(".?")
				: new Regex(argumentRegexPattern);
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			m_logger.LogInformation($"Wait until process {m_processName} {m_argument} started");
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(ProcessPollInterval, cancellationToken);
				var windowHandle = FindMatchingWindowHandle();
				if (windowHandle != IntPtr.Zero)
				{
					try
					{
						m_logger.LogInformation($"Start sharing screen of {m_processName} - {m_windowTitle}");
						if (m_moveOffScreen)
						{
							NativeWindowHelper.MoveWindowOffScreen(windowHandle);
						}
						var server = new AppWindowShareServer(windowHandle, m_port, m_logFrameRate, m_logger);
						server.StartSharing();
						m_logger.LogInformation($"Stop sharing screen of  {m_processName} - {m_windowTitle}");
					}
					catch (Exception e)
					{
						m_logger.LogError(e.ToString());
					}
					m_logger.LogInformation($"Wait until process {m_processName} {m_argument} started");
				}
			}
		}

		private IntPtr FindMatchingWindowHandle()
		{
			if (!string.IsNullOrWhiteSpace(m_windowTitle))
			{
				return NativeWindowHelper.FindWindowHandle(m_windowTitle);
			}
			if (!string.IsNullOrWhiteSpace(m_processName))
			{
				foreach (var process in System.Diagnostics.Process.GetProcesses()
					         .Where(x => x.ProcessName == m_processName))
				{
					try
					{
						var commandLine = GetCommandLineArgs(process);
						if (m_argument.IsMatch(commandLine))
						{
							if (process.MainWindowHandle == IntPtr.Zero)
							{
								throw new InvalidOperationException($"Process {m_processName} does not have an associated main window. Use window title argument instead");
							}

							return process.MainWindowHandle;
						}
					}
					catch (Exception e)
					{
						m_logger.LogWarning(
							$"Could not get startup information for process {process.ProcessName} - is service running with required privileges?");
					}
				}
			}
			return IntPtr.Zero;
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
