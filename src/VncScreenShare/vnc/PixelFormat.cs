using SharpDX.DXGI;

namespace VncScreenShare.Vnc
{
	internal record PixelFormat(byte BitsPerPixel, byte Depth, bool BigEndian, bool TrueColor, ushort RedMax,
		ushort GreenMax, ushort BlueMax, byte RedShift, byte GreenShift, byte BlueShift)
	{
		public PixelFormat() 
			: this(0, 0, false, false, 0, 0, 0, 0, 0, 0)
		{
		}

		public SharpDX.DXGI.Format DxPixelFormat
		{
			get
			{
				if (!TrueColor)
				{
					return Format.Unknown;
				}

				if (BitsPerPixel == 16)
				{
					if (RedMax == 2 << 5 && GreenMax == 2 << 6 && BlueMax == 2 << 5)
					{
						return Format.B5G6R5_UNorm;
					}
				}

				if (BitsPerPixel == 32)
				{
					if (RedMax == 0xFF && GreenMax == 0xFF && BlueMax == 0xFF)
					{
						return Format.B8G8R8A8_UNorm;
					}
				}
				return Format.Unknown;
			}
		}

		public static PixelFormat ReadFromStream(VncStreamReader streamReader)
		{
			var pf = new PixelFormat()
			{
				BitsPerPixel = streamReader.ReadByte(),
				Depth = streamReader.ReadByte(),

				BigEndian = streamReader.ReadByte() == 1,
				TrueColor = streamReader.ReadByte() == 1,

				RedMax = streamReader.ReadUInt16(),
				GreenMax = streamReader.ReadUInt16(),
				BlueMax = streamReader.ReadUInt16(),

				RedShift = streamReader.ReadByte(),
				GreenShift = streamReader.ReadByte(),
				BlueShift = streamReader.ReadByte(),
			};
			// padding
			streamReader.SkipBytes(3);
			return pf;
		}

		public void WriteToStream(VncStreamWriter writer)
		{
			writer.Write(BitsPerPixel);
			writer.Write(Depth);

			writer.Write(BigEndian ? (byte)1 : (byte)0);
			writer.Write(TrueColor ? (byte)1 : (byte)0);

			writer.Write(RedMax);
			writer.Write(GreenMax);
			writer.Write(BlueMax);

			writer.Write(RedShift);
			writer.Write(GreenShift);
			writer.Write(BlueShift);

			writer.WritePadding(3); // padding
		}
	}
}
