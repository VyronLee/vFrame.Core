namespace vFrame.Core.Patch
{
    public enum UpdateState
    {
        /// <summary>
        /// 未检测
        /// </summary>
        UNCHECKED,

        /// <summary>
        /// 正在下载版本号
        /// </summary>
        DOWNLOADING_VERSION,

        /// <summary>
        /// 正在下载manifest
        /// </summary>
        DOWNLOADING_MANIFEST,

        /// <summary>
        /// 需要更新
        /// </summary>
        NEED_UPDATE,

        /// <summary>
        /// 正在更新
        /// </summary>
        UPDATING,

        /// <summary>
        /// 已经更新
        /// </summary>
        UP_TO_DATE,

        /// <summary>
        /// 更新失败
        /// </summary>
        FAIL_TO_UPDATE,

        /// <summary>
        /// 需要强更
        /// </summary>
        NEED_FORCE_UPDATE,
    }
}