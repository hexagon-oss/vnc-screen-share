using System.Buffers.Binary;

namespace VncScreenShare.Vnc
{
	internal class VncStreamWriter
	{
		private readonly BinaryWriter m_binaryWriter;

		public VncStreamWriter(Stream stream)
		{
			m_binaryWriter = new BinaryWriter(stream);
		}

		public void Write(byte value)
		{
			m_binaryWriter.Write(value);
		}

		public void Write(ushort value)
		{
			m_binaryWriter.Write(BinaryPrimitives.ReverseEndianness(value));
		}

		public void WritePadding(int count)
		{
			m_binaryWriter.Write(stackalloc byte[count]);
		}

		public void Write(uint value)
		{
			m_binaryWriter.Write(BinaryPrimitives.ReverseEndianness(value));
		}

		public void Write(byte[] bytes)
		{
			m_binaryWriter.Write(bytes);
		}

		public void Write(byte[] bytes, int length)
		{
			m_binaryWriter.Write(bytes,0, length);
		}

		public void Flush()
		{
			m_binaryWriter.Flush();
		}

		public void Dispose()
		{
			m_binaryWriter.Dispose();
		}
	}
}
