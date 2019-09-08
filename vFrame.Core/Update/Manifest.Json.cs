using System;
using System.Collections.Generic;

namespace vFrame.Core.Update
{
    public partial class Manifest
    {
        [Serializable]
        public class ManifestJson
        {
            public string build_number;
            public string game_version;
            public string assets_version;
            public List<AssetInfo> assets = new List<AssetInfo>();
        }
        
        [Serializable]
        public class AssetInfo
        {
            public string md5;
            public string fileName;
            public ulong size;
            public DownloadState downloadState = DownloadState.UNSTARTED;
        }

        [Serializable]
        public enum DownloadState
        {
            UNSTARTED,
            DOWNLOADING,
            DOWNLOADED,
            SUCCEED
        }
    }
}