using System;

namespace vFrame.Core.Unity.Patch
{
    [Serializable]
    public enum DownloadState
    {
        UNSTARTED,
        DOWNLOADING,
        DOWNLOADED,
        SUCCEED
    }
}