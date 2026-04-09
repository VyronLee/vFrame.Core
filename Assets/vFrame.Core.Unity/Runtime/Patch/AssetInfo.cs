// ------------------------------------------------------------
//         File: AssetInfo.cs
//        Brief: Metadata for a single patch asset entry.
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
    public class AssetInfo
    {
        /// <summary>
        ///     MD5 hash of the asset file.
        /// </summary>
        public string md5;

        /// <summary>
        ///     Relative file name of the asset.
        /// </summary>
        public string fileName;

        /// <summary>
        ///     Size of the asset in bytes.
        /// </summary>
        public ulong size;

        /// <summary>
        ///     Current download state of the asset.
        /// </summary>
        public DownloadState downloadState = DownloadState.NotStarted;
    }
}
