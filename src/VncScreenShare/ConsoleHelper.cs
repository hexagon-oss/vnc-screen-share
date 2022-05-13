using System.Runtime.InteropServices;


namespace VncScreenShare
{
	internal static class ConsoleHelper
	{
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		const int SW_HIDE = 0;
		// public const int SW_SHOW = 5;

		public static void HideConsoleWindow()
		{
			ShowWindow(GetConsoleWindow(), SW_HIDE);
		}
	}
}
