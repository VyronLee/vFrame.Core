namespace vFrame.Core.Patch
{
    public class VersionManifest : Manifest
    {
        /// <summary>
        /// CDN url
        /// </summary>
        public string cdnUrl;

        /// <summary>
        /// Game package download url.
        /// </summary>
        public string downloadUrl;

        /// <summary>
        /// On load manifest
        /// </summary>
        protected override void OnLoadJson(ManifestJson json) {
            cdnUrl = json.cdnUrl;
            downloadUrl = json.downloadUrl;
        }
    }
}