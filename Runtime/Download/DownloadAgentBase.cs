using System;
using System.IO;

namespace vFrame.Core.Unity.Download
{
    public abstract class DownloadAgentBase : IDownloadAgent
    {
        private DownloadTask m_Task;

        public DownloadTask Task {
            get { return m_Task; }
        }

        public bool TaskDone { get; private set; }

        public int Timeout { get; set; }

        public float ProgressUpdateInterval { get; set; }

        public ulong DownloadedSizeDelta { get; private set; }
        public abstract ulong DownloadedSize { get; }
        public abstract ulong TotalSize { get; }
        public abstract float Progress { get; }

        public event Action<IDownloadAgent> DownloadAgentStart;
        public event Action<IDownloadAgent> DownloadAgentUpdate;
        public event Action<IDownloadAgent> DownloadAgentSuccess;
        public event Action<IDownloadAgent, string> DownloadAgentFailure;

        private ulong m_LastDownloadedSize;

        public void Start(DownloadTask task) {
            m_Task = task;
            m_LastDownloadedSize = 0;
            TaskDone = false;

            EnsureDownloadDirectory();
            NotifyStart();

            OnStart();
        }

        protected virtual void OnStart() {
        }

        public void Stop() {
            m_Task = null;

            OnStop();
        }

        protected virtual void OnStop() {
        }

        public void Update(float elapseSeconds) {
            if (m_Task == null) {
                return;
            }

            OnUpdate(elapseSeconds);

            DownloadedSizeDelta = Math.Max(DownloadedSize - m_LastDownloadedSize, 0);
            m_LastDownloadedSize = DownloadedSize;
        }

        protected virtual void OnUpdate(float elapseSeconds) {
        }

        private void EnsureDownloadDirectory() {
            if (string.IsNullOrEmpty(m_Task.DownloadPath)) {
                return;
            }

            var dir = Path.GetDirectoryName(m_Task.DownloadPath);
            if (string.IsNullOrEmpty(dir)) {
                return;
            }

            Directory.CreateDirectory(dir);
        }

        protected void NotifyStart() {
            if (DownloadAgentStart != null) {
                DownloadAgentStart(this);
            }
        }

        protected void NotifyUpdate() {
            if (DownloadAgentUpdate != null) {
                DownloadAgentUpdate(this);
            }
        }

        protected void NotifyComplete() {
            if (DownloadAgentSuccess != null) {
                DownloadAgentSuccess(this);
            }

            TaskDone = true;
        }

        protected void NotifyError(string errorMsg) {
            if (DownloadAgentFailure != null) {
                DownloadAgentFailure(this, errorMsg);
            }

            TaskDone = true;
        }
    }
}