using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VncScreenShare.Capture;
using VncScreenShare.Vnc;

namespace VncScreenShare
{
    internal class AppWindowShareServer
    {
        private readonly IntPtr m_windowHandle;
        private readonly int m_port;
        private readonly bool m_logFrameRate;
        private readonly ILogger m_logger;

        public AppWindowShareServer(IntPtr windowHandle, int port, bool logFrameRate, ILogger logger)
        {
            m_windowHandle = windowHandle;
            m_port = port;
            m_logFrameRate = logFrameRate;
            m_logger = logger;
        }

        public void StartSharing()
        {
            if (m_windowHandle != IntPtr.Zero)
            {
                var process = Process.GetProcessById(NativeWindowHelper.GetProcessIdForWindowHandle(m_windowHandle));
                using var windowCapture = new WindowCapture(m_windowHandle);
                windowCapture.StartCapture();
                var serverSocket = new TcpListener(IPAddress.Any, m_port);
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    serverSocket.Start();
                    process.EnableRaisingEvents = true;
                    process.Exited += (sender, args) =>
                    {
                        cancellationTokenSource.Cancel();
                        serverSocket.Stop();
                    };
                    using var socket = serverSocket.AcceptSocket();
                    using var connection = new VncClientConnection(new NetworkStream(socket), windowCapture,
                        new FrameRateLogger(m_logFrameRate, m_logger));
                    m_logger.LogInformation($"Client {socket.RemoteEndPoint} connected");
                    connection.HandleClient(cancellationTokenSource.Token);
                    m_logger.LogInformation($"Client communication ended");
                }
                catch (SocketException exc)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        m_logger.LogInformation(FormattableString.Invariant($"Exception after cancelling sharing: {exc}"));
                    }
                    else
                    {
                        m_logger.LogWarning(exc.ToString());
                    }
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
