#if NET6_0_OR_GREATER
using System.Buffers.Binary;
#endif

namespace VncScreenShare.Vnc
{
	internal class VncStreamReader
	{
		private readonly BinaryReader m_binaryReader;

		public VncStreamReader(Stream stream)
		{
			m_binaryReader = new BinaryReader(stream);
		}
		public byte ReadByte()
		{
			return m_binaryReader.ReadByte();
		}

		public ushort ReadUInt16()
		{
			return BinaryPrimitives.ReverseEndianness(m_binaryReader.ReadUInt16());
		}

		public void SkipBytes(int i)
		{
			m_binaryReader.Read(stackalloc byte[i]);
		}

		public uint ReadUInt32()
		{
			return BinaryPrimitives.ReverseEndianness(m_binaryReader.ReadUInt32());
		}

		public int ReadInt32()
		{
			return BinaryPrimitives.ReverseEndianness(m_binaryReader.ReadInt32());
		}

		public byte[] ReadBytes(int count)
		{
			return m_binaryReader.ReadBytes(count);
		}

		public void Dispose()
		{
			m_binaryReader.Dispose();
		}
	}
}
