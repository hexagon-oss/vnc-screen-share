using System.Buffers;
using System.Text;
using VncScreenShare.Capture;
using VncScreenShare.vnc.Rectangles;

namespace VncScreenShare.Vnc
{
	/// <summary>
	/// Implements parts of the Remote Famebuffer Protocol
	/// https://github.com/rfbproto/rfbproto
	/// </summary>
	internal sealed class VncClientConnection : IDisposable
	{
		private readonly Stream m_stream;
		private readonly WindowCapture m_windowCapture;
		private readonly FrameRateLogger m_frameRateLogger;
		private readonly VncStreamReader m_reader;
		private readonly VncStreamWriter m_writer;
		private readonly FrameEncoder m_frameEncoder;
		private PixelFormat m_pixelFormat;

		public VncClientConnection(Stream stream, WindowCapture windowCapture, FrameRateLogger frameRateLogger)
		{
			m_stream = stream;
			m_windowCapture = windowCapture;
			m_frameRateLogger = frameRateLogger;
			m_reader = new VncStreamReader(stream);
			m_writer = new VncStreamWriter(stream);
			m_pixelFormat = new PixelFormat(32, 24, false, true, 255, 255, 255, 16, 8, 0);
			m_frameEncoder = new FrameEncoder(new [] { ImgEncoding.RawEncoding , ImgEncoding.EncodingZlib});
		}

		public void HandleClient(CancellationToken token)
		{
			DoHandShake();
			SendInitMessage();

			while (!token.IsCancellationRequested)
			{
				ClientMessages message = (ClientMessages)m_reader.ReadByte();
				switch (message)
				{
					case ClientMessages.SetPixelFormat:
						HandleSetPixelFormat();
						break;
					case ClientMessages.SetEncodings:
						HandleSetEncodings();
						break;
					case ClientMessages.FramebufferUpdateRequest:
						HandleFrameUpdateRequest(token);
						break;
					case ClientMessages.PointerEvent:
						HandlePointEvent();
						break;
					case ClientMessages.KeyEvent:
						HandleKeyEvent();
						break;
					case ClientMessages.ClientCutText:
						HandleClientCutTextEvent();
						break;
					default:
						Console.WriteLine($"--> Not handled :{message}");
						break;
					
				}
			}

		}

		private void HandleClientCutTextEvent()
		{
			// values ignored
			m_reader.SkipBytes(3);
			var len = Convert.ToInt32(m_reader.ReadUInt32());
			m_reader.ReadBytes(len);
		}

		private void HandleKeyEvent()
		{
			bool pressed = (m_reader.ReadByte() == 1);
			m_reader.SkipBytes(2);
			uint keysym = m_reader.ReadUInt32();
		}

		private void HandlePointEvent()
		{
			byte buttonMask = m_reader.ReadByte();
			ushort X = m_reader.ReadUInt16();
			ushort Y = m_reader.ReadUInt16();
			// TODO: Pointer event
		}

		/// <summary>
		/// https://datatracker.ietf.org/doc/html/rfc6143#section-7.6.1
		/// </summary>
		private void HandleFrameUpdateRequest(CancellationToken token)
		{
			bool incremental = m_reader.ReadByte() == 1;
			ushort x = m_reader.ReadUInt16();
			ushort y = m_reader.ReadUInt16();
			ushort width = m_reader.ReadUInt16();
			ushort height = m_reader.ReadUInt16();

			var frame = m_windowCapture.WaitForFrame(token, m_pixelFormat);

			 var rectangle = m_frameEncoder.EncodeFrame(frame);

			//write update message
			m_writer.Write((byte)ServerMessages.FramebufferUpdate);
			m_writer.WritePadding(1); // padding
			//rectangle count  -- currently only one
			m_writer.Write((ushort)1);

			//rectangle data
			rectangle.WriteData(m_writer);
			m_writer.Flush();
			ArrayPool<byte>.Shared.Return(frame.data);
			m_frameRateLogger.OnFrameReceived();
		}


		/// <summary>
		/// https://datatracker.ietf.org/doc/html/rfc6143#section-7.5.1
		/// </summary>
		private void HandleSetPixelFormat()
		{
			m_reader.SkipBytes(3); // padding
			m_pixelFormat = PixelFormat.ReadFromStream(m_reader);
			Console.WriteLine($"New pixel format {m_pixelFormat}");
		}

		/// <summary>
		/// https://datatracker.ietf.org/doc/html/rfc6143#section-7.5.2
		/// https://github.com/TurboVNC/tightvnc/blob/main/vnc_winsrc/rfb/rfbproto.h
		/// </summary>
		private void HandleSetEncodings()
		{
			m_reader.SkipBytes(1); // padding
			int count = m_reader.ReadUInt16();
			for (int i = 0; i < count; i++)
			{
				List<ImgEncoding> encodings = new List<ImgEncoding>();
				encodings.Add((ImgEncoding)m_reader.ReadInt32());
				m_frameEncoder.ClientEncodingsConfigured(encodings);
			}
		}

		private void DoHandShake()
		{
			// write version
			m_writer.Write(Encoding.ASCII.GetBytes("RFB 003.008\n"));
			m_writer.Flush();

			// read client version
			var version = Encoding.ASCII.GetString(m_reader.ReadBytes(12));
			Console.WriteLine($"Client connected version: {version}");

			// write authentication type (1 no auth)
			m_writer.Write((byte)1);
			m_writer.Write((byte)1);
			m_writer.Flush();

			//read from client
			var foo = m_reader.ReadByte();
			m_writer.Write(0u); // write auth success code

			//read share flag --> ignored
			var share = m_reader.ReadByte();
		}

		private void SendInitMessage()
		{
			if (m_windowCapture.Width == 0 || m_windowCapture.Height == 0)
			{
				throw new InvalidOperationException("Window was closed");
			}
			m_writer.Write((ushort)m_windowCapture.Width);
			m_writer.Write((ushort)m_windowCapture.Height);
			m_pixelFormat.WriteToStream(m_writer);

			m_writer.Write((uint)m_windowCapture.WindowName.Length);
			m_writer.Write(Encoding.ASCII.GetBytes(m_windowCapture.WindowName));
			m_writer.Flush();
		}

		public void Dispose()
		{
			m_stream.Dispose();
			m_reader.Dispose();
			m_writer.Dispose();

			m_frameRateLogger.Dispose();
        }
	}
}