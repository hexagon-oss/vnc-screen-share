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
}
