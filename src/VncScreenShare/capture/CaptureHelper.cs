using System.Runtime.InteropServices;
using Windows.Graphics.Capture;

namespace VncScreenShare.Capture
{
	public static class CaptureHelper
	{
		static readonly Guid GraphicsCaptureItemGuid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");

		[ComImport]
		[Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[ComVisible(true)]
		interface IGraphicsCaptureItemInterop
		{
			IntPtr CreateForWindow(
				[In] IntPtr window,
				[In] ref Guid iid);
		}

		public static GraphicsCaptureItem CreateItemForWindow(IntPtr hwnd)
		{
			var factory = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
			var itemPointer = factory.CreateForWindow(hwnd, GraphicsCaptureItemGuid);
			var item = GraphicsCaptureItem.FromAbi(itemPointer);
			Marshal.Release(itemPointer);
			return item;
		}
	}
}
