using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using VncScreenShare.Capture;
using VncScreenShare.Vnc;

namespace VncScreenShare
{
	internal class AppWindowShareServer
	{
		private readonly IntPtr m_windowHandle;
		private readonly int m_port;
		private readonly bool m_logFrameRate;

		public AppWindowShareServer(IntPtr windowHandle, int port, bool logFrameRate)
		{
			m_windowHandle = windowHandle;
			m_port = port;
			m_logFrameRate = logFrameRate;
		}

		public void StartSharing()
		{
			if (m_windowHandle != IntPtr.Zero)
			{
				var process = Process.GetProcessById(NativeWindowHelper.GetProcessIdForWindowHandle(m_windowHandle));
				using var windowCapture = new WindowCapture(m_windowHandle);
				windowCapture.StartCapture();
				var serverSocket = new TcpListener(IPAddress.Any, m_port);
				try
				{
					serverSocket.Start();
					CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
					process.EnableRaisingEvents = true;
					process.Exited += (sender, args) =>
					{
						cancellationTokenSource.Cancel();
						serverSocket.Stop();
					};
					using var socket = serverSocket.AcceptSocket();
					using var connection = new VncClientConnection(new NetworkStream(socket), windowCapture,
						new FrameRateLogger(m_logFrameRate));
					connection.HandleClient(cancellationTokenSource.Token);

				}
				catch (SocketException)
				{
				}
				finally
				{
					serverSocket.Stop();
				}
			}
			else
			{
				throw new InvalidOperationException("Process does not have a MainWindow");
			}
		}
	}
}
