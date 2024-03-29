using System;

namespace vFrame.Core.Unity.Patch
{
    [Serializable]
    public enum DownloadState
    {
        NotStarted,
        Downloading,
        Downloaded,
        Succeed
    }
}