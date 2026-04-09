// ------------------------------------------------------------
//         File: IDownloadAgent.cs
//        Brief: Interface for download agents that manage the
//               lifecycle and progress of a single download task.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-09 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Defines the contract for a download agent responsible for
    /// starting, updating, and stopping an individual download task.
    /// </summary>
    public interface IDownloadAgent
    {
        /// <summary>
        /// Gets the current download task managed by this agent.
        /// </summary>
        DownloadTask Task { get; }

        /// <summary>
        /// Gets a value indicating whether the current task has completed,
        /// either successfully or with a failure.
        /// </summary>
        bool TaskDone { get; }

        /// <summary>
        /// Gets the number of bytes downloaded since the last update.
        /// </summary>
        ulong DownloadedSizeDelta { get; }

        /// <summary>
        /// Gets the total number of bytes downloaded so far.
        /// </summary>
        ulong DownloadedSize { get; }

        /// <summary>
        /// Gets the total expected size of the download in bytes.
        /// </summary>
        ulong TotalSize { get; }

        /// <summary>
        /// Gets the download progress as a normalized value between 0 and 1.
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Gets or sets the timeout in seconds for the download request.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Gets or sets the minimum interval in seconds between progress update callbacks.
        /// </summary>
        float ProgressUpdateInterval { get; set; }

        /// <summary>
        /// Raised when the download agent starts processing a task.
        /// </summary>
        event Action<IDownloadAgent> DownloadAgentStart;

        /// <summary>
        /// Raised on each progress update during the download.
        /// </summary>
        event Action<IDownloadAgent> DownloadAgentUpdate;

        /// <summary>
        /// Raised when the download completes successfully.
        /// </summary>
        event Action<IDownloadAgent> DownloadAgentSuccess;

        /// <summary>
        /// Raised when the download fails. The second parameter contains the error message.
        /// </summary>
        event Action<IDownloadAgent, string> DownloadAgentFailure;

        /// <summary>
        /// Starts downloading the specified task.
        /// </summary>
        /// <param name="task">The download task to execute.</param>
        void Start(DownloadTask task);

        /// <summary>
        /// Stops the current download and releases resources.
        /// </summary>
        void Stop();

        /// <summary>
        /// Called every frame to drive the download forward and update progress metrics.
        /// </summary>
        /// <param name="elapseSeconds">Time elapsed since the last update, in seconds.</param>
        void Update(float elapseSeconds);
    }
}
