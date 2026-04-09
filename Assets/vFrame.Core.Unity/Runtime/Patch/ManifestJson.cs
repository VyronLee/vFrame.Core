// ------------------------------------------------------------
//         File: ManifestJson.cs
//        Brief: Serializable JSON schema for patch manifest files.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-16 22:32:14
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;

namespace vFrame.Core.Unity
{
    [Serializable]
    public class ManifestJson
    {
        /// <summary>
        ///     Build number of the patch.
        /// </summary>
        public string buildNumber;

        /// <summary>
        ///     Engine version string.
        /// </summary>
        public string engineVersion;

        /// <summary>
        ///     Assets version string.
        /// </summary>
        public string assetsVersion;

        /// <summary>
        ///     CDN URL for downloading patch files.
        /// </summary>
        public string cdnUrl;

        /// <summary>
        ///     Download URL for the full game package.
        /// </summary>
        public string downloadUrl;

        /// <summary>
        ///     List of asset entries included in the manifest.
        /// </summary>
        public List<AssetInfo> assets = new List<AssetInfo>();
    }
}
