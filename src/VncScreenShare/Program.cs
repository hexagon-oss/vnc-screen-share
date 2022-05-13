using System.Diagnostics;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace VncScreenShare
{
	internal class Program
	{
		static void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<CommandLineOptions, WatchMode>(args)
				.WithParsed<CommandLineOptions>(Run)
				.WithParsed<WatchMode>(RunInWatchMode)
				.WithNotParsed(InvalidCommandLine);

		}

		private static void Run(CommandLineOptions options)
		{
			if (Process.GetProcesses().All(x => x.Id != options.ProcessId))
			{
				InvalidCommandLine(null);
				return;
			}

			var server = new AppWindowShareServer(options.ProcessId, options.Port, options.LogFrameRate);
			server.StartSharing();
		}

		private static void InvalidCommandLine(IEnumerable<Error> obj)
		{
			Console.WriteLine("Invalid argument - specify process id");
			foreach (var process in System.Diagnostics.Process.GetProcesses()
				         .Where(x => !string.IsNullOrWhiteSpace(x.MainWindowTitle))
				         .OrderBy(x => x.MainWindowTitle))
			{
				Console.WriteLine($"PID: {process.Id}\t{process.MainWindowTitle}");
			}
		}

		private static void RunInWatchMode(WatchMode args)
		{
			if (args.HideWindow)
			{
				ConsoleHelper.HideConsoleWindow();
			}
			using IHost host = Host.CreateDefaultBuilder()
				.ConfigureLogging(logging =>
				{
					logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information);
				})
				.ConfigureServices(services =>
				{
					services.AddHostedService((s) => new ScreenShareBackgroundService(args.ProcessName, args.CommandLine, args.Port, false, s.GetRequiredService<ILogger<ScreenShareBackgroundService>>()));
					services.Configure<EventLogSettings>(settings =>
					{
						settings.SourceName = "VncScreenShare";
					});
				})
				.Build();
			host.Run();
		}
	}
}