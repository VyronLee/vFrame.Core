// ------------------------------------------------------------
//         File: PatchOptions.cs
//        Brief: Configuration options for the Patcher.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-16 22:32:14
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    public class PatchOptions
    {
        /// <summary>
        ///     CDN base URL for downloading patch files.
        /// </summary>
        public string cdnUrl;

        /// <summary>
        ///     Sub-directory on the CDN server.
        /// </summary>
        public string cdnDir;

        /// <summary>
        ///     Whether to delete cached files when they are out of date.
        /// </summary>
        public bool deleteCacheOutOfDate;

        /// <summary>
        ///     File name of the manifest file.
        /// </summary>
        public string manifestFilename;

        /// <summary>
        ///     Local directory where downloaded files are stored.
        /// </summary>
        public string storagePath;

        /// <summary>
        ///     Download timeout in milliseconds.
        /// </summary>
        public int timeout = int.MaxValue;

        /// <summary>
        ///     File name of the version file.
        /// </summary>
        public string versionFilename;

        /// <summary>
        ///     Direct URL to the version file (overrides CDN-based resolution).
        /// </summary>
        public string versionUrl;
    }
}
