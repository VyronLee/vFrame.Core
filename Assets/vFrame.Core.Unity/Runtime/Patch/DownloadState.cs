// ------------------------------------------------------------
//         File: DownloadState.cs
//        Brief: Enum representing the download lifecycle of a patch asset.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-16 22:32:14
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core.Unity
{
    [Serializable]
    public enum DownloadState
    {
        /// <summary>
        ///     Download has not started yet.
        /// </summary>
        NotStarted,

        /// <summary>
        ///     Download is in progress.
        /// </summary>
        Downloading,

        /// <summary>
        ///     Download completed but not yet validated.
        /// </summary>
        Downloaded,

        /// <summary>
        ///     Download completed and hash-validated successfully.
        /// </summary>
        Succeed
    }
}
