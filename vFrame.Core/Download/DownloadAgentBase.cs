using System;

namespace vFrame.Core
{
    public abstract class DownloadAgentBase : IDownloadAgent
    {
        private DownloadTask m_Task;

        public DownloadTask Task
        {
            get { return m_Task; }
        }
        
        public bool TaskDone { get; private set; }

        public float Timeout { get; set; }
        
        public float ProgressUpdateInterval { get; set; }

        private readonly DownloadSpeedCounter m_SpeedCounter = new DownloadSpeedCounter();

        public float Speed
        {
            get { return m_SpeedCounter.Speed; }
        }

        public abstract ulong DownloadedSize { get; }
        public abstract ulong TotalSize { get; }
        public abstract float Progress { get; }

        public event Action<IDownloadAgent> DownloadAgentStart;
        public event Action<IDownloadAgent> DownloadAgentUpdate;
        public event Action<IDownloadAgent> DownloadAgentSuccess;
        public event Action<IDownloadAgent, string> DownloadAgentFailure;

        public void Start(DownloadTask task)
        {
            m_Task = task;
            TaskDone = false;
            NotifyStart();

            OnStart();
        }

        protected virtual void OnStart() { }

        public void Stop()
        {
            m_Task = null;
            m_SpeedCounter.Reset();
            
            OnStop();
        }

        protected virtual void OnStop() { }

        public void Update(float elapseSeconds)
        {
            if (m_Task == null)
            {
                return;
            }
            
            m_SpeedCounter.Update(elapseSeconds);

            OnUpdate(elapseSeconds);
        }

        protected virtual void OnUpdate(float elapseSeconds) { }

        protected void NotifyStart()
        {
            if (DownloadAgentStart != null)
            {
                DownloadAgentStart(this);
            }
        }

        protected void NotifyUpdate()
        {
            m_SpeedCounter.RecordDownloadSize(DownloadedSize);
            m_SpeedCounter.Update(0);

            if (DownloadAgentUpdate != null)
            {
                DownloadAgentUpdate(this);
            }
        }

        protected void NotifyComplete()
        {
            m_SpeedCounter.RecordDownloadSize(TotalSize);
            m_SpeedCounter.Update(0);
            
            if (DownloadAgentSuccess != null)
            {
                DownloadAgentSuccess(this);
            }

            TaskDone = true;
        }

        protected void NotifyError(string errorMsg)
        {
            if (DownloadAgentFailure != null)
            {
                DownloadAgentFailure(this, errorMsg);
            }

            TaskDone = true;
        }
    }
}