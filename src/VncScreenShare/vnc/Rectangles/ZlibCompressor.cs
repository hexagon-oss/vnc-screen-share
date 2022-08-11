using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace VncScreenShare.vnc.Rectangles
{
	/// <summary>
	/// A single zlib stream have to be used for the whole session
	/// https://github.com/rfbproto/rfbproto/blob/master/rfbproto.rst#zlib-encoding
	/// </summary>
	internal class ZlibCompressor
	{
		private readonly DeflaterOutputStream m_compressionStream;
		private readonly MemoryStream m_bufferStream;

		public ZlibCompressor()
		{
			m_bufferStream = new MemoryStream();
			m_compressionStream = new DeflaterOutputStream(m_bufferStream);
		}

		public byte[] CompressFrame(byte[] rawData, int rawLength, out int compressedLength)
		{
			m_bufferStream.Position = 0;
			m_compressionStream.Write(rawData, 0, rawLength);
			m_compressionStream.Flush();
			compressedLength = (int)m_bufferStream.Position;
			return m_bufferStream.ToArray();
		}
	}
}