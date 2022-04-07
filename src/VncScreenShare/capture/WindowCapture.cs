using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using SharpDX.Direct3D11;
using VncScreenShare.Vnc;

namespace VncScreenShare.Capture
{
	internal class WindowCapture : IDisposable
	{
		private readonly IntPtr m_hwnd;
		private GraphicsCaptureItem m_captureItem;
		private readonly IDirect3DDevice m_d3dDevice;
		private Direct3D11CaptureFramePool m_framePool;
		private GraphicsCaptureSession m_session;
		private readonly object m_textureLock;
		private readonly Device m_sharpDxDevice;

		private Texture2D m_stagingTexture;
		private bool m_pendingFrameRequest;

		public WindowCapture(IntPtr hwnd)
		{
			m_textureLock = new object();
			m_d3dDevice = Direct3D11Helper.CreateDevice();
			m_sharpDxDevice = Direct3D11Helper.CreateSharpDXDevice(m_d3dDevice);
			m_hwnd = hwnd;
		}

		public ushort Width => (ushort) m_captureItem.Size.Width;

		public ushort Height => (ushort)m_captureItem.Size.Height;
		public string WindowName => m_captureItem.DisplayName;

		public void StartCapture()
		{
			m_captureItem = CaptureHelper.CreateItemForWindow(m_hwnd);
			if (m_captureItem == null)
			{
				throw new InvalidOperationException("Start capture failed");
			}
			Console.WriteLine($"Start capture window: {m_captureItem.DisplayName}");
			m_framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
				m_d3dDevice,
				DirectXPixelFormat.B8G8R8A8UIntNormalized,
				2,
				m_captureItem.Size);
			m_session = m_framePool.CreateCaptureSession(m_captureItem);

#pragma warning disable CA1416 // Validate platform compatibility
			if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent(typeof(GraphicsCaptureSession).FullName, nameof(GraphicsCaptureSession.IsCursorCaptureEnabled)))
			{
				// https://docs.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscapturesession.isborderrequired?view=winrt-22000
				m_session.IsCursorCaptureEnabled = false;
#pragma warning restore CA1416 // Validate platform compatibility
			}

			m_framePool.FrameArrived += OnFrameArrived;
			m_session.StartCapture();
		}

		private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
		{
			using var frame = sender.TryGetNextFrame();

			if (m_pendingFrameRequest)
			{
				lock (m_textureLock)
				{
					if (m_pendingFrameRequest)
					{
						var texture = GetOrCreateTexture(frame.Surface.Description);
						using var bitmap = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface);
						m_sharpDxDevice.ImmediateContext.CopyResource(bitmap, texture);
						m_pendingFrameRequest = false;
						Monitor.PulseAll(m_textureLock);
					}
				}
			}
		}

		public CapturedFrame WaitForFrame(CancellationToken cancellationToken, PixelFormat pixelFormat)
		{
			lock (m_textureLock)
			{
				m_pendingFrameRequest = true;
				while (m_pendingFrameRequest)
				{
					cancellationToken.ThrowIfCancellationRequested();
					Monitor.Wait(m_textureLock);
				}
				// Map our texture and get the bits
				var mapped = m_sharpDxDevice.ImmediateContext.MapSubresource(m_stagingTexture, 0, MapMode.Read, MapFlags.None);
				var result = PixelConverter.Convert(mapped.DataPointer, mapped.RowPitch, m_stagingTexture.Description.Width, m_stagingTexture.Description.Height, pixelFormat);
				m_sharpDxDevice.ImmediateContext.UnmapSubresource(m_stagingTexture, 0);
				return result;
			}
		}

		private Texture2D GetOrCreateTexture(Direct3DSurfaceDescription surfaceDescription)
		{
			if (m_stagingTexture == null ||
			    m_stagingTexture.Description.Width != surfaceDescription.Width ||
			    m_stagingTexture.Description.Height != surfaceDescription.Height)
			{
				lock (m_textureLock)
				{
					Console.WriteLine($"New source buffer w:{surfaceDescription.Width} h: {surfaceDescription.Height}");
					m_stagingTexture?.Dispose();
					var description = new SharpDX.Direct3D11.Texture2DDescription
					{
						Width = surfaceDescription.Width,
						Height = surfaceDescription.Height,
						MipLevels = 1,
						ArraySize = 1,
						Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
						SampleDescription = new SharpDX.DXGI.SampleDescription()
						{
							Count = 1,
							Quality = 0
						},
						Usage = ResourceUsage.Staging,
						BindFlags = BindFlags.None,
						CpuAccessFlags = CpuAccessFlags.Read,
						OptionFlags = ResourceOptionFlags.None
					};
					m_stagingTexture = new Texture2D(m_sharpDxDevice, description);
				}
			}

			return m_stagingTexture;
		}

		public void Dispose()
		{
			m_session?.Dispose();
			m_framePool?.Dispose();
			m_stagingTexture?.Dispose();
			m_sharpDxDevice.Dispose();
		}
	}
}
