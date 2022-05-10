using CommandLine;

namespace VncScreenShare
{
	internal class CommandLineOptions
	{
		[Option( "pid", Required = true, HelpText = "Process id")]
		public int ProcessId
		{
			get;
			set;
		}

		[Option( "lr", Required = false, HelpText = "logs frame rate")]
		public bool LogFrameRate
		{
			get;
			set;
		}

		[Option('p', "port", Required = false, HelpText = "VNC port (default 5900)", Default = 5900)]
		public int Port
		{
			get;
			set;
		}
	}

	[Verb("watch", HelpText = "Launch as service and share screen automatically")]
	class WatchMode
	{
		[Option('n', "process_name", Required = true, HelpText = "Process name WITHOUT .exe")]
		public string ProcessName
		{
			get;
			set;
		}

		[Option('c', "command_line", Required = true, HelpText = "command line match pattern (Regex)")]
		public string CommandLine
		{
			get;
			set;
		}

		[Option('p', "port", Required = false, HelpText = "VNC port (default 5900)", Default = 5900)]
		public int Port
		{
			get;
			set;
		}
	}
}
