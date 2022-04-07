// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using CommandLine;

namespace VncScreenShare
{
	internal class Program
	{
		static void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
				.WithParsed(Run)
				.WithNotParsed(InvalidCommandLine);

		}
		
		private static void Run(CommandLineOptions options)
		{
			if (Process.GetProcesses().All(x => x.Id != options.ProcessId))
			{
				InvalidCommandLine(null);
				return;
			}

			var server = new AppWindowShareServer(options.ProcessId, options);
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
	}
}