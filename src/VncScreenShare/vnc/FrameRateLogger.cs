using System.Diagnostics;
using System.Timers;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace VncScreenShare.Vnc
{
	internal class FrameRateLogger : IDisposable
	{
		private readonly bool m_optionsLogFrameRate;
        private readonly ILogger m_logger;
        private Stopwatch m_stopwatch;
		private int m_counter = 0;
        private DateTime? m_lastFrameReceiveTime;
        private DateTime m_lastErrorLog;
        private readonly Timer m_monitorTimer;

        public FrameRateLogger(bool optionsLogFrameRate, ILogger logger)
		{
			m_optionsLogFrameRate = optionsLogFrameRate;
            m_logger = logger;

            m_monitorTimer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
			m_monitorTimer.Elapsed += OnMonitorTimerOnElapsed;
            m_lastErrorLog = DateTime.MinValue;
            m_monitorTimer.Start();
        }

        private void OnMonitorTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!m_lastFrameReceiveTime.HasValue)
            {
                return;
            }

            if (DateTime.Now - m_lastFrameReceiveTime > TimeSpan.FromSeconds(2) &&
                m_lastFrameReceiveTime != m_lastErrorLog)
            {
                m_logger.LogError("No FrameUpdateRequest received for more than 2 sec.");
                m_lastErrorLog = m_lastFrameReceiveTime.Value;
            }
        }

        public void OnFrameReceived()
        {
            m_lastFrameReceiveTime = DateTime.Now;

            if (m_optionsLogFrameRate)
			{
				if (m_stopwatch == null)
				{
					m_stopwatch = Stopwatch.StartNew();
				}

				m_counter++;
				if (m_stopwatch.Elapsed > TimeSpan.FromSeconds(5))
				{
					var framesPerSec = m_counter / m_stopwatch.Elapsed.TotalSeconds;
					Console.WriteLine($"frames / sec {framesPerSec:F1}");
					m_stopwatch.Restart();
					m_counter = 0;
				}
			}
		}

        public void Dispose()
        {
            m_monitorTimer.Dispose();
        }
    }
}
