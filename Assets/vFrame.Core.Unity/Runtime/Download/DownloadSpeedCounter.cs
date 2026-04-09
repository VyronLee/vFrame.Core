// ------------------------------------------------------------
//         File: DownloadSpeedCounter.cs
//        Brief: Tracks downloaded bytes over fixed time intervals
//                to compute an aggregate download speed.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:00:00
//    Copyright: Copyright (c) 2019, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    public class DownloadSpeedCounter
    {
        private const float UPDATE_INTERVAL = 1.0f;
        private ulong m_CurrentDownloadSize;
        private float m_ElapseSeconds;

        private ulong m_LastDownloadSize;

        /// <summary>
        /// Gets the current download speed in bytes per second,
        /// updated once per <see cref="UPDATE_INTERVAL"/> seconds.
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Accumulates elapsed time and recalculates speed when the
        /// interval threshold is reached.
        /// </summary>
        /// <param name="elapseSeconds">Delta time since the last update call.</param>
        public void Update(float elapseSeconds) {
            m_ElapseSeconds += elapseSeconds;
            if (m_ElapseSeconds < UPDATE_INTERVAL) {
                return;
            }

            var size = m_CurrentDownloadSize - m_LastDownloadSize;
            Speed = (float)(size / (decimal)m_ElapseSeconds);

            m_ElapseSeconds = 0;
            m_LastDownloadSize = m_CurrentDownloadSize;
        }

        /// <summary>
        /// Adds the specified number of downloaded bytes to the running total.
        /// </summary>
        /// <param name="size">Number of bytes downloaded since the last call.</param>
        public void AddDownloadSize(ulong size) {
            m_CurrentDownloadSize += size;
        }

        /// <summary>
        /// Resets the counter, clearing all accumulated bytes and speed.
        /// </summary>
        public void Reset() {
            Speed = 0f;
            m_ElapseSeconds = 0f;
            m_LastDownloadSize = 0;
            m_CurrentDownloadSize = 0;
        }
    }
}
