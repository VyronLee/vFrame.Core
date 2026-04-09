// ------------------------------------------------------------
//         File: UpdateState.cs
//        Brief: Enum representing the current state of the patch update process.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-16 22:32:14
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    public enum UpdateState
    {
        /// <summary>
        ///     Update has not been checked yet.
        /// </summary>
        Unchecked,

        /// <summary>
        ///     Version file is being downloaded.
        /// </summary>
        DownloadingVersion,

        /// <summary>
        ///     Manifest file is being downloaded.
        /// </summary>
        DownloadingManifest,

        /// <summary>
        ///     An asset update is available.
        /// </summary>
        NeedUpdate,

        /// <summary>
        ///     Asset download is in progress.
        /// </summary>
        Updating,

        /// <summary>
        ///     All assets are already up to date.
        /// </summary>
        UpToDate,

        /// <summary>
        ///     The update process has failed.
        /// </summary>
        FailToUpdate,

        /// <summary>
        ///     A full game update (force update) is required.
        /// </summary>
        NeedForceUpdate
    }
}
