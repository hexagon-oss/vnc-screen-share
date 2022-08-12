using System.Buffers;
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
		private readonly ArrayPoolStream m_bufferStream;

		public ZlibCompressor()
		{
			m_bufferStream = new ArrayPoolStream();
			m_compressionStream = new DeflaterOutputStream(m_bufferStream);
		}

		public byte[] CompressFrame(byte[] rawData, int rawLength, out int compressedLength)
		{
			var buffer = ArrayPool<byte>.Shared.Rent(rawLength);
			m_bufferStream.SetBuffer(buffer);
			m_compressionStream.Write(rawData, 0, rawLength);
			m_compressionStream.Flush();
			compressedLength = (int)m_bufferStream.Position;
			return buffer;
		}

		/// <summary>
		/// To avoid copy buffer this stream writes to different buffer blocks
		/// </summary>
		public class ArrayPoolStream : Stream
		{
			private MemoryStream m_wrapped;

			public override bool CanRead => m_wrapped.CanRead;
			public override bool CanSeek => m_wrapped.CanSeek;
			public override bool CanWrite => m_wrapped.CanWrite;
			public override long Length => m_wrapped.Length;
			public override long Position { get => m_wrapped.Position; set => m_wrapped.Position = value; }

			public ArrayPoolStream()
			{
				m_wrapped = new MemoryStream(0);
			}
			public override void Flush()
			{
				m_wrapped.Flush();
			}
			public override void Write(byte[] buffer, int offset, int count)
			{
				m_wrapped.Write(buffer, offset, count);
			}
			public void SetBuffer(byte[] buffer)
			{
				m_wrapped = new MemoryStream(buffer);
			}
			public override int Read(byte[] buffer, int offset, int count)
			{
				return m_wrapped.Read(buffer, offset, count);
			}
			public override long Seek(long offset, SeekOrigin origin)
			{
				return m_wrapped.Seek(offset, origin);
			}
			public override void SetLength(long value)
			{
				m_wrapped.SetLength(value);
			}
		}
	}
}