using System.Diagnostics;

namespace VncScreenShare.Vnc
{
	internal class FrameRateLogger
	{
		private readonly bool m_optionsLogFrameRate;
		private Stopwatch m_stopwatch;
		private int m_counter = 0;

		public FrameRateLogger(bool optionsLogFrameRate)
		{
			m_optionsLogFrameRate = optionsLogFrameRate;

		}

		public void OnFrameReceived()
		{
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
	}
}
