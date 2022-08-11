using VncScreenShare.Capture;
using VncScreenShare.Vnc;

namespace VncScreenShare.vnc.Rectangles
{
	class ZlibRectangle : Rectangle
	{
		private readonly ZlibCompressor m_zlibCompressor;


		public ZlibRectangle(CapturedFrame capturedFrame, ZlibCompressor zlibCompressor) : base(capturedFrame)
		{
			m_zlibCompressor = zlibCompressor;
		}

		public override void WriteData(VncStreamWriter writer)
		{
			byte[] compressedData = m_zlibCompressor.CompressFrame(m_frame.data, m_frame.length, out var compressedLength);

			base.WriteData(writer);
			writer.Write((uint)ImgEncoding.EncodingZlib);
			writer.Write((uint)compressedLength);
			writer.Write(compressedData, compressedLength);
		}
	}
}