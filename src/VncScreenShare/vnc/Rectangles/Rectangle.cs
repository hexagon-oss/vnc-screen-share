using VncScreenShare.Capture;
using VncScreenShare.Vnc;

namespace VncScreenShare.vnc.Rectangles
{
	abstract class Rectangle
	{
		protected readonly CapturedFrame m_frame;

		protected Rectangle(CapturedFrame capturedFrame)
		{
			m_frame = capturedFrame;
		}

		public virtual void WriteData(VncStreamWriter writer)
		{
			//rectangle data
			writer.Write((ushort)0); // x
			writer.Write((ushort)0); // y
			writer.Write((ushort)m_frame.width);
			writer.Write((ushort)m_frame.height);
		}
	}
}