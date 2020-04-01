using System;

namespace vFrame.Core.Patch
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