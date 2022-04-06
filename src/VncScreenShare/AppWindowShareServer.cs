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
		private readonly CommandLineOptions m_options;

		public AppWindowShareServer(int processId, CommandLineOptions options)
		{
			m_processId = processId;
			m_port = options.Port;
			m_options = options;
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
				serverSocket.Start();

				CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
				process.Exited += (sender, args) =>
				{
					cancellationTokenSource.Cancel();
				};

				using var socket = serverSocket.AcceptSocket();
				using var connection = new VncClientConnection(new NetworkStream(socket), windowCapture, new FrameRateLogger(m_options.LogFrameRate));
				connection.HandleClient(cancellationTokenSource.Token);
			}
			else
			{
				throw new InvalidOperationException("Process does not have a MainWindow");
			}
		}
	}
}
