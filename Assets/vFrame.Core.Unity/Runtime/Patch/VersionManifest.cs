// ------------------------------------------------------------
//         File: VersionManifest.cs
//        Brief: Manifest specialization that also carries CDN and download URLs.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-16 22:32:14
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    public class VersionManifest : Manifest
    {
        /// <summary>
        ///     CDN URL for downloading patch files.
        /// </summary>
        public string cdnUrl;

        /// <summary>
        ///     Download URL for the full game package.
        /// </summary>
        public string downloadUrl;

        /// <summary>
        ///     Extracts CDN and download URLs from the parsed JSON manifest.
        /// </summary>
        /// <param name="json">The parsed manifest JSON data.</param>
        protected override void OnLoadJson(ManifestJson json) {
            cdnUrl = json.cdnUrl;
            downloadUrl = json.downloadUrl;
        }
    }
}
