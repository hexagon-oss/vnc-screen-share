using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VncScreenShare
{
	internal class NativeWindowHelper
	{
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, SetWindowPosFlags wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

		private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

		[DllImport("USER32.DLL")]
		private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

		[DllImport("USER32.DLL")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("USER32.DLL")]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
		}

		const int HWND_BOTTOM = 1;
        [Flags]
        public enum SetWindowPosFlags : uint
        {
	        SWP_NOREDRAW = 0x0008,
	        SWP_NOSIZE = 0x0001,
        }


        public static void MoveWindowOffScreen(IntPtr handle)
        {
	        GetWindowRect(handle, out var rect);
	        if (rect.Right == 0 || rect.Bottom == 0)
	        {
				return;
	        }
			SetWindowPos(handle, HWND_BOTTOM, -rect.Right, -rect.Bottom, 0, 0, SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOSIZE);
        }


        public static int GetProcessIdForWindowHandle(IntPtr windowHandle)
        {
	        GetWindowThreadProcessId(windowHandle, out var processId);
			return processId;
		}

        public static IntPtr FindWindowHandle(string title)
        {
	        IntPtr result = IntPtr.Zero;
			EnumWindows((wnd, param) =>
			{
				int length = GetWindowTextLength(wnd);
				if (length == 0) return true;

				StringBuilder builder = new StringBuilder(length);
				GetWindowText(wnd, builder, length + 1);
				var name = builder.ToString();
				if (name == title)
				{
					result = wnd;
					return false;
				}
				return true;
			}, 0);
			return result;
        }

        public static Dictionary<int, List<string>> EnumerateWindows()
        {
			var result = new Dictionary<int, List<string>> ();
			EnumWindows((wnd, param) =>
			{
				int length = GetWindowTextLength(wnd);
				if (length == 0) return true;

				StringBuilder builder = new StringBuilder(length);
				GetWindowText(wnd, builder, length + 1);

				var processId = GetProcessIdForWindowHandle(wnd);
				if (!result.TryGetValue(processId, out var windowsTitles))
				{
					windowsTitles = new List<string>();
					result[processId] = windowsTitles;
				}
				windowsTitles.Add(builder.ToString());
				return true;
			}, 0);
			return result;
        }

	}
}
