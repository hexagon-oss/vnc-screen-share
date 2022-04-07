using System.Buffers;
using SharpDX.DXGI;
using VncScreenShare.Vnc;

namespace VncScreenShare.Capture
{
	internal static class PixelConverter
	{

		public static CapturedFrame Convert(IntPtr mappedDataPointer, int sourceStride, int width, int height, PixelFormat pixelFormat)
		{
			if (pixelFormat.DxPixelFormat == Format.B5G6R5_UNorm)
			{
				return Convert_B5G6R5(mappedDataPointer, sourceStride, width, height, pixelFormat);
			}

			if (pixelFormat.DxPixelFormat == Format.B8G8R8A8_UNorm)
			{
				return Convert_B8G8R8A8(mappedDataPointer, sourceStride, width, height, pixelFormat);
			}

			throw new NotImplementedException($"Pixel Format {pixelFormat} not supported");
		}

		private static unsafe CapturedFrame Convert_B8G8R8A8(IntPtr mappedDataPointer, int sourceStride, int width, int height, PixelFormat pixelFormat)
		{
			var source = mappedDataPointer;
			var bytes = ArrayPool<byte>.Shared.Rent(width * height * 4);
			fixed (byte* bytesPointer = bytes)
			{
				var dest = (IntPtr)bytesPointer;
				var destStride = width * 4;

				for (int i = 0; i < height; i++)
				{
					SharpDX.Utilities.CopyMemory(dest, source, destStride);

					source = IntPtr.Add(source, sourceStride);
					dest = IntPtr.Add(dest, destStride);
				}
			}
			return new CapturedFrame(){data =  bytes, height = height, width = width, length = width * height * 4 };
		}

		private static CapturedFrame Convert_B5G6R5(IntPtr mappedDataPointer, int sourceStride, int width, int height, PixelFormat pixelFormat)
		{
			// Allocate some memory to hold our copy
			var dest = new byte[2 * width * height];
			unsafe
			{
				fixed (byte* pDest = dest)
				{
					byte* pSourceLineStart = (byte*)mappedDataPointer;
					byte* pDestLineStart = pDest;


					var destStride = width * 2;
					for (int row = 0; row < height; row++)
					{
						uint* pSourceData = (uint*)pSourceLineStart;
						uint* pDestData = (uint*)pDestLineStart;

						for (int c = 0; c < width; c++)
						{
							uint src = pSourceData[c]; // B8G8R8A8U
							uint b = ((src >> 24) & 0xFF);
							uint g = (src >> 16) & 0xFF;
							uint r = (src >> 8) & 0xFF;
							b = b * pixelFormat.BlueMax / 0xFF;
							g = g * pixelFormat.GreenMax / 0xFF;
							r = r * pixelFormat.RedMax / 0xFF;

							// B5G6R5UIntNormalized
							pDestData[c] = (ushort)(((b << 11) & 0xF800) |
													((g << 5) & 0x07E0) |
													(r & 0x001F));
							pDestData[c] = pSourceData[c];
						}

						pSourceLineStart += sourceStride;
						pDestLineStart += destStride;
					}
				}
			}
			return new CapturedFrame() { data = dest, height = height, width = width, length = width * height * 2 };
		}
	}
}
