using System;

namespace vFrame.Core.Patch
{
    [Serializable]
    public class AssetInfo
    {
        public string md5;
        public string fileName;
        public ulong size;
        public DownloadState downloadState = DownloadState.UNSTARTED;
    }
}