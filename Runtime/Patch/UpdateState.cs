namespace vFrame.Core.Unity.Patch
{
    public enum UpdateState
    {
        /// <summary>
        ///     未检测
        /// </summary>
        Unchecked,

        /// <summary>
        ///     正在下载版本号
        /// </summary>
        DownloadingVersion,

        /// <summary>
        ///     正在下载manifest
        /// </summary>
        DownloadingManifest,

        /// <summary>
        ///     需要更新
        /// </summary>
        NeedUpdate,

        /// <summary>
        ///     正在更新
        /// </summary>
        Updating,

        /// <summary>
        ///     已经更新
        /// </summary>
        UpToDate,

        /// <summary>
        ///     更新失败
        /// </summary>
        FailToUpdate,

        /// <summary>
        ///     需要强更
        /// </summary>
        NeedForceUpdate
    }
}