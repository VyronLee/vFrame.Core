namespace vFrame.Core.Download
{
    public class DownloadSpeedCounter
    {
        private const float UPDATE_INTERVAL = 1.0f;

        private ulong m_LastDownloadSize;
        private ulong m_CurrentDownloadSize;
        private float m_ElapseSeconds;

        public float Speed { get; private set; }

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

        public void AddDownloadSize(ulong size) {
            m_CurrentDownloadSize += size;
        }

        public void Reset() {
            Speed = 0f;
            m_ElapseSeconds = 0f;
            m_LastDownloadSize = 0;
            m_CurrentDownloadSize = 0;
        }
    }
}