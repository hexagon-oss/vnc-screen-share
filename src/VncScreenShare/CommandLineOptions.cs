using CommandLine;

namespace VncScreenShare
{
	[Verb("share", HelpText = "shares the specified process id")]
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

		[Option('c', "command_line", Required = false, HelpText = "command line match pattern (Regex)")]
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

		[Option('h', "hide", Required = false, HelpText = "Hides the console window", Default = false)]
		public bool HideWindow
		{
			get;
			set;
		}

		[Option('m', "move_offscreen", Required = false, HelpText = "Moves the captured window off screen", Default = false)]
		public bool MoveOffScreen
		{
			get;
			set;
		}

		[Option("lr", Required = false, HelpText = "logs frame rate")]
		public bool LogFrameRate
		{
			get;
			set;
		}


		[Option('w', "window_title", Required = false, Default = null, HelpText = "If a window title is specified the command line and process name argument is ignored")]
		public string WindowTitle { get; set; }
	}


	[Verb("listwindow", HelpText = "Lists all open windows with associated process id")]
	class ListWindow
	{
	}
}
