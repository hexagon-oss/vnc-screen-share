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
			CommandLine.Parser.Default.ParseArguments<CommandLineOptions, WatchMode, ListWindow>(args)
				.WithParsed<CommandLineOptions>(Run)
				.WithParsed<WatchMode>(RunInWatchMode)
				.WithParsed<ListWindow>(RunListWindow)
				.WithNotParsed(InvalidCommandLine);

		}

		private static void Run(CommandLineOptions options)
		{
			if (Process.GetProcesses().All(x => x.Id != options.ProcessId || x.MainWindowHandle == IntPtr.Zero))
			{
				InvalidCommandLine(null);
				return;
			}

			var windowHandle = Process.GetProcessById(options.ProcessId).MainWindowHandle;
			var server = new AppWindowShareServer(windowHandle, options.Port, options.LogFrameRate);
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
					services.AddHostedService((s) => new ScreenShareBackgroundService(args.ProcessName, args.CommandLine, args.Port, false, args.MoveOffScreen, args.WindowTitle, s.GetRequiredService<ILogger<ScreenShareBackgroundService>>()));
					services.Configure<EventLogSettings>(settings =>
					{
						settings.SourceName = "VncScreenShare";
					});
				})
				.Build();
			host.Run();
		}

		private static void RunListWindow(ListWindow obj)
		{
			foreach (var pair in NativeWindowHelper.EnumerateWindows())
			{
				var process = Process.GetProcesses().FirstOrDefault(x => pair.Key == x.Id);
				if (process == null)
				{
					continue;
				}
				Console.WriteLine($"Process ID {process.Id} - {process.ProcessName}");
				foreach (var title in pair.Value)
				{
					Console.WriteLine($"--> \"{title}\"");
				}
			}
		}
	}
}