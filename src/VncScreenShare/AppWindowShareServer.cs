using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using VncScreenShare.Capture;
using VncScreenShare.Vnc;

namespace VncScreenShare
{
	internal class AppWindowShareServer
	{
		private readonly int m_processId;
		private readonly int m_port;
		private readonly bool m_logFrameRate;

		public AppWindowShareServer(int processId, int port, bool logFrameRate)
		{
			m_processId = processId;
			m_port = port;
			m_logFrameRate = logFrameRate;
		}

		public void StartSharing()
		{
			var process = Process.GetProcessById(m_processId);
			var hwnd = process?.MainWindowHandle;
			if (hwnd != IntPtr.Zero)
			{
				using var windowCapture = new WindowCapture(hwnd.Value);
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
					using var connection = new VncClientConnection(new NetworkStream(socket), windowCapture, new FrameRateLogger(m_logFrameRate));
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
