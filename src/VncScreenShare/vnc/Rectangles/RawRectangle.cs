using VncScreenShare.Capture;
using VncScreenShare.Vnc;

namespace VncScreenShare.vnc.Rectangles
{
	class RawRectangle : Rectangle
	{
		public RawRectangle(CapturedFrame capturedFrame) : base(capturedFrame)
		{
		}

		public override void WriteData(VncStreamWriter writer)
		{
			base.WriteData(writer);
			writer.Write((uint)ImgEncoding.RawEncoding);
			writer.Write(m_frame.data, m_frame.length);
		}
	}
}