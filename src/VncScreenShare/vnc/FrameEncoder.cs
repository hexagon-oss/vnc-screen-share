using VncScreenShare.Capture;
using VncScreenShare.vnc.Rectangles;

namespace VncScreenShare.Vnc
{
	internal class FrameEncoder
	{
		private readonly IEnumerable<ImgEncoding> m_supportedEncodings;
		private readonly ZlibCompressor m_zlibCompressor;
		private readonly HashSet<ImgEncoding> m_clientEncodings;

		public FrameEncoder(IEnumerable<ImgEncoding> supportedEncodings)
		{
			m_supportedEncodings = supportedEncodings;
			m_clientEncodings = new HashSet<ImgEncoding>();
			m_zlibCompressor = new ZlibCompressor();
		}

		public ImgEncoding? ImageEncoding
		{
			get;
			private set;
		}


		public void ClientEncodingsConfigured(IReadOnlyCollection<ImgEncoding> clientEncodings)
		{
			foreach (var encoding in clientEncodings)
			{
				if (encoding > 0)
				{
					m_clientEncodings.Add(encoding);
				}
			}
		}

		public Rectangle EncodeFrame(CapturedFrame capturedFrame)
		{
			if (ImageEncoding == null)
			{
				ChooseEncoding();
			}

			if(ImageEncoding == ImgEncoding.EncodingZlib) return new ZlibRectangle(capturedFrame, m_zlibCompressor);
			return new RawRectangle(capturedFrame);
		}

		private void ChooseEncoding()
		{
			Console.WriteLine($"Client supports Encodings: {string.Join(";", m_clientEncodings)}");
			Console.WriteLine($"Server supports Encodings: {string.Join(";", m_supportedEncodings)}");
			ImageEncoding = ImgEncoding.RawEncoding;
			foreach (var encoding in m_clientEncodings)
			{
				if (m_supportedEncodings.Contains(encoding))
				{
					ImageEncoding = encoding;
					break;
				}
			}
			Console.WriteLine($"Used Encoding -> {ImageEncoding}");
		}

	}
}

