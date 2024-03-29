using System;
using System.IO;

namespace vFrame.Core.Unity.Download
{
    public abstract class DownloadAgentBase : IDownloadAgent
    {
        public DownloadTask Task { get; private set; }

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

        private ulong LastDownloadedSize { get; set; }

        public void Start(DownloadTask task) {
            Task = task;
            TaskDone = false;
            LastDownloadedSize = 0;

            EnsureDownloadDirectory();
            NotifyStart();

            OnStart();
        }

        public void Stop() {
            Task = null;
            OnStop();
        }

        public void Update(float elapseSeconds) {
            if (Task == null) {
                return;
            }

            OnUpdate(elapseSeconds);

            DownloadedSizeDelta = Math.Max(DownloadedSize - LastDownloadedSize, 0);
            LastDownloadedSize = DownloadedSize;
        }

        protected virtual void OnStart() { }

        protected virtual void OnStop() { }

        protected virtual void OnUpdate(float elapseSeconds) { }

        private void EnsureDownloadDirectory() {
            if (string.IsNullOrEmpty(Task.DownloadPath)) {
                return;
            }

            var dir = Path.GetDirectoryName(Task.DownloadPath);
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