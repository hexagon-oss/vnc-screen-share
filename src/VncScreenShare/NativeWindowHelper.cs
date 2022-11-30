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

		/// <summary>
		/// Creates and initializes the magnifier run-time objects.
		/// https://docs.microsoft.com/en-us/windows/win32/api/magnification/nf-magnification-maginitialize
		/// </summary>
		/// <returns> Returns TRUE if successful, or FALSE otherwise. </returns>
		[DllImport("Magnification.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool MagInitialize();

		/// <summary>
		/// Destroys the magnifier run-time objects.
		/// https://docs.microsoft.com/en-us/windows/win32/api/magnification/nf-magnification-maguninitialize
		/// </summary>
		/// <returns> Returns TRUE if successful, or FALSE otherwise. </returns>

		[DllImport("Magnification.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool MagUninitialize();

		/// <summary>
		/// Changes the color transformation matrix associated with the full-screen magnifier.
		/// </summary>
		/// <param name="pEffect"> The new color transformation matrix. This parameter must not be NULL. </param>
		/// <returns> Returns TRUE if successful, or FALSE otherwise. </returns>
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetMagnificationDesktopColorEffect(ref ColorEffect pEffect);

		/// <summary>
		/// Changes the color transformation matrix associated with the full-screen magnifier.
		/// </summary>
		/// <param name="pEffect"> Returns color transformation matrix.</param>
		/// <returns> Returns TRUE if successful, or FALSE otherwise. </returns>
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetMagnificationDesktopColorEffect(IntPtr hWnd, out ColorEffect pEffect);

		// MagGetColorEffect

		/// <summary>
		/// 4x4 color matrix
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct ColorEffect
		{
			public float transform00;
			public float transform01;
			public float transform02;
			public float transform03;
			public float transform04;
			public float transform10;
			public float transform11;
			public float transform12;
			public float transform13;
			public float transform14;
			public float transform20;
			public float transform21;
			public float transform22;
			public float transform23;
			public float transform24;
			public float transform30;
			public float transform31;
			public float transform32;
			public float transform33;
			public float transform34;
			public float transform40;
			public float transform41;
			public float transform42;
			public float transform43;
			public float transform44;

			public ColorEffect(float[][] matrix)
			{
				transform00 = matrix[0][0];
				transform10 = matrix[1][0];
				transform20 = matrix[2][0];
				transform30 = matrix[3][0];
				transform40 = matrix[4][0];
				transform01 = matrix[0][1];
				transform11 = matrix[1][1];
				transform21 = matrix[2][1];
				transform31 = matrix[3][1];
				transform41 = matrix[4][1];
				transform02 = matrix[0][2];
				transform12 = matrix[1][2];
				transform22 = matrix[2][2];
				transform32 = matrix[3][2];
				transform42 = matrix[4][2];
				transform03 = matrix[0][3];
				transform13 = matrix[1][3];
				transform23 = matrix[2][3];
				transform33 = matrix[3][3];
				transform43 = matrix[4][3];
				transform04 = matrix[0][4];
				transform14 = matrix[1][4];
				transform24 = matrix[2][4];
				transform34 = matrix[3][4];
				transform44 = matrix[4][4];
			}
		}

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
