// ------------------------------------------------------------
//         File: DownloadEventArgs.cs
//        Brief: Data structure carrying download progress,
//                error, and status information for a single
//                download task event notification.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    /// <summary>
    ///     Holds event data reported during a download lifecycle,
    ///     including progress, byte counts, and optional error text.
    /// </summary>
    public struct DownloadEventArgs
    {
        /// <summary>
        ///     Gets or sets the serial identifier of the download task.
        /// </summary>
        public int SerialId { get; set; }

        /// <summary>
        ///     Gets or sets the error message when a download fails; otherwise <c>null</c>.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        ///     Gets or sets the number of bytes downloaded so far.
        /// </summary>
        public ulong DownloadedSize { get; set; }

        /// <summary>
        ///     Gets or sets the total expected size of the download in bytes.
        /// </summary>
        public ulong TotalSize { get; set; }

        /// <summary>
        ///     Gets or sets the download progress as a normalized value between 0 and 1.
        /// </summary>
        public float Progress { get; set; }

        /// <summary>
        ///     Gets or sets an optional user data object attached to the event.
        /// </summary>
        public object UserData { get; set; }
    }
}
