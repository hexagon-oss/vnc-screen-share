using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VncScreenShare
{
	internal class InvertedScreen
	{
		private NativeWindowHelper.ColorEffect m_colorEffect;

		public InvertedScreen(IntPtr hWnd)
		{
			if (NativeWindowHelper.GetMagnificationDesktopColorEffect(hWnd, out var colorEffect))
			{
				m_colorEffect = colorEffect;
			}
		}
	}
}
