// ------------------------------------------------------------
//         File: DownloadTask.cs
//        Brief: Represents a single download operation, carrying the
//                target URL, local save path, and lifecycle events.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:00:00
//    Copyright: Copyright (c) 2019, VyronLee
// ============================================================

using System;

namespace vFrame.Core.Unity
{
    public sealed class DownloadTask
    {
        private static int _taskId;

        /// <summary>
        /// Creates a new download task with an auto-incrementing identifier.
        /// </summary>
        /// <param name="downloadPath">Local file path where the downloaded data will be saved.</param>
        /// <param name="downloadUrl">Remote URL to download from.</param>
        /// <param name="userData">Optional user data attached to the task.</param>
        public DownloadTask(string downloadPath, string downloadUrl, object userData) {
            TaskId = _taskId++;
            DownloadPath = downloadPath;
            DownloadUrl = downloadUrl;
            UserData = userData;
        }

        /// <summary>
        /// Gets the unique identifier of this download task.
        /// </summary>
        public int TaskId { get; }

        /// <summary>
        /// Gets the local file path where downloaded data will be saved.
        /// </summary>
        public string DownloadPath { get; }

        /// <summary>
        /// Gets the remote URL to download from.
        /// </summary>
        public string DownloadUrl { get; }

        /// <summary>
        /// Gets the optional user data associated with this task.
        /// </summary>
        public object UserData { get; }

        /// <summary>
        /// Raised when the download starts.
        /// </summary>
        public event Action<DownloadEventArgs> DownloadStart;

        /// <summary>
        /// Raised when the download reports progress.
        /// </summary>
        public event Action<DownloadEventArgs> DownloadUpdate;

        /// <summary>
        /// Raised when the download completes successfully.
        /// </summary>
        public event Action<DownloadEventArgs> DownloadSuccess;

        /// <summary>
        /// Raised when the download fails.
        /// </summary>
        public event Action<DownloadEventArgs> DownloadFailure;

        /// <summary>
        /// Invokes the <see cref="DownloadStart"/> event.
        /// </summary>
        /// <param name="args">Event arguments describing the download start.</param>
        public void NotifyStart(DownloadEventArgs args) {
            if (DownloadStart != null) {
                DownloadStart(args);
            }
        }

        /// <summary>
        /// Invokes the <see cref="DownloadUpdate"/> event.
        /// </summary>
        /// <param name="args">Event arguments describing the download progress.</param>
        public void NotifyUpdate(DownloadEventArgs args) {
            if (DownloadUpdate != null) {
                DownloadUpdate(args);
            }
        }

        /// <summary>
        /// Invokes the <see cref="DownloadSuccess"/> event.
        /// </summary>
        /// <param name="args">Event arguments describing the completed download.</param>
        public void NotifySuccess(DownloadEventArgs args) {
            if (DownloadSuccess != null) {
                DownloadSuccess(args);
            }
        }

        /// <summary>
        /// Invokes the <see cref="DownloadFailure"/> event.
        /// </summary>
        /// <param name="args">Event arguments describing the download failure.</param>
        public void NotifyFailure(DownloadEventArgs args) {
            if (DownloadFailure != null) {
                DownloadFailure(args);
            }
        }
    }
}
