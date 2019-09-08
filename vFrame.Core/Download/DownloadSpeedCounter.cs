namespace vFrame.Core
{
    public class DownloadSpeedCounter
    {
        private const float UPDATE_INTERVAL = 1.0f;

        private ulong m_LastDownloadSize;
        private ulong m_CurrentDownloadSize;
        private float m_TotalElapseSeconds;
        private float m_ElapseSeconds;

        public float Speed { get; private set; }
        
        public void Update(float elapseSeconds)
        {
            m_TotalElapseSeconds += elapseSeconds;
            if (m_TotalElapseSeconds < UPDATE_INTERVAL)
            {
                Speed = m_CurrentDownloadSize / m_TotalElapseSeconds;
            }
            
            m_ElapseSeconds += elapseSeconds;
            if (m_ElapseSeconds >= UPDATE_INTERVAL)
            {
                Speed = (m_CurrentDownloadSize - m_LastDownloadSize) / m_ElapseSeconds;
                m_ElapseSeconds = 0;
                m_LastDownloadSize = m_CurrentDownloadSize;
            }
        }

        public void RecordDownloadSize(ulong size)
        {
            m_CurrentDownloadSize = size;
        }

        public void Reset()
        {
            Speed = 0f;
            m_TotalElapseSeconds = 0f;
            m_ElapseSeconds = 0f;
            m_LastDownloadSize = 0;
            m_CurrentDownloadSize = 0;
        }
    }
}