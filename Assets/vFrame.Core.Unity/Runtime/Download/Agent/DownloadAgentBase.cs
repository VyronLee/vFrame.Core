// ------------------------------------------------------------
//         File: DownloadAgentBase.cs
//        Brief: Abstract base class for download agents providing
//               common lifecycle, progress tracking, and event
//               notification logic.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-09 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;
using System.IO;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Provides a base implementation of <see cref="IDownloadAgent"/> with
    /// common progress tracking, directory creation, and event notification.
    /// Subclasses override abstract and virtual hooks for platform-specific behavior.
    /// </summary>
    public abstract class DownloadAgentBase : IDownloadAgent
    {
        /// <inheritdoc />
        public DownloadTask Task { get; private set; }

        /// <inheritdoc />
        public bool TaskDone { get; private set; }

        /// <inheritdoc />
        public int Timeout { get; set; }

        /// <inheritdoc />
        public float ProgressUpdateInterval { get; set; }

        /// <inheritdoc />
        public ulong DownloadedSizeDelta { get; private set; }

        /// <inheritdoc />
        public abstract ulong DownloadedSize { get; }

        /// <inheritdoc />
        public abstract ulong TotalSize { get; }

        /// <inheritdoc />
        public abstract float Progress { get; }

        /// <inheritdoc />
        public event Action<IDownloadAgent> DownloadAgentStart;

        /// <inheritdoc />
        public event Action<IDownloadAgent> DownloadAgentUpdate;

        /// <inheritdoc />
        public event Action<IDownloadAgent> DownloadAgentSuccess;

        /// <inheritdoc />
        public event Action<IDownloadAgent, string> DownloadAgentFailure;

        private ulong LastDownloadedSize { get; set; }

        /// <inheritdoc />
        public void Start(DownloadTask task) {
            Task = task;
            TaskDone = false;
            LastDownloadedSize = 0;

            EnsureDownloadDirectory();
            NotifyStart();

            OnStart();
        }

        /// <inheritdoc />
        public void Stop() {
            Task = null;
            OnStop();
        }

        /// <inheritdoc />
        public void Update(float elapseSeconds) {
            if (Task == null) {
                return;
            }

            OnUpdate(elapseSeconds);

            DownloadedSizeDelta = Math.Max(DownloadedSize - LastDownloadedSize, 0);
            LastDownloadedSize = DownloadedSize;
        }

        /// <summary>
        /// Called when the download agent starts. Override to perform
        /// platform-specific initialization.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// Called when the download agent stops. Override to perform
        /// platform-specific cleanup.
        /// </summary>
        protected virtual void OnStop() { }

        /// <summary>
        /// Called every frame while a task is active. Override to drive
        /// the actual download progress.
        /// </summary>
        /// <param name="elapseSeconds">Time elapsed since the last update, in seconds.</param>
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

        /// <summary>
        /// Raises the <see cref="DownloadAgentStart"/> event.
        /// </summary>
        protected void NotifyStart() {
            if (DownloadAgentStart != null) {
                DownloadAgentStart(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="DownloadAgentUpdate"/> event.
        /// </summary>
        protected void NotifyUpdate() {
            if (DownloadAgentUpdate != null) {
                DownloadAgentUpdate(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="DownloadAgentSuccess"/> event and marks the task as done.
        /// </summary>
        protected void NotifyComplete() {
            if (DownloadAgentSuccess != null) {
                DownloadAgentSuccess(this);
            }
            TaskDone = true;
        }

        /// <summary>
        /// Raises the <see cref="DownloadAgentFailure"/> event with the specified error
        /// message and marks the task as done.
        /// </summary>
        /// <param name="errorMsg">A description of the error that occurred.</param>
        protected void NotifyError(string errorMsg) {
            if (DownloadAgentFailure != null) {
                DownloadAgentFailure(this, errorMsg);
            }
            TaskDone = true;
        }
    }
}
