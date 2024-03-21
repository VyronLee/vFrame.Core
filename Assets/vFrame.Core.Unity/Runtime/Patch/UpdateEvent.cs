namespace vFrame.Core.Unity.Patch
{
    public class UpdateEvent
    {
        public enum EventCode
        {
            /// <summary>
            ///     找不到本地配置
            /// </summary>
            ErrorNoLocalManifest,

            /// <summary>
            ///     下载版本号失败
            /// </summary>
            ErrorDownloadVersion,

            /// <summary>
            ///     解析版本号失败
            /// </summary>
            ErrorParseVersion,

            /// <summary>
            ///     下载manifest失败
            /// </summary>
            ErrorDownloadManifest,

            /// <summary>
            ///     解析manifest失败
            /// </summary>
            ErrorParseManifest,

            /// <summary>
            ///     发现新版本
            /// </summary>
            NewAssetsVersionFound,

            /// <summary>
            ///     发现新版本
            /// </summary>
            NewGameVersionFound,

            /// <summary>
            ///     已最新
            /// </summary>
            AlreadyUpToDate,

            /// <summary>
            ///     更新进度
            /// </summary>
            UpdateProgression,

            /// <summary>
            ///     单个资源下载成功
            /// </summary>
            AssetUpdated,

            /// <summary>
            ///     单个资源下载失败
            /// </summary>
            ErrorDownloadFailed,

            /// <summary>
            ///     更新成功
            /// </summary>
            UpdateFinished,

            /// <summary>
            ///     更新失败
            /// </summary>
            UpdateFailed,

            /// <summary>
            ///     哈希检测
            /// </summary>
            HashStart,

            /// <summary>
            ///     哈希进度
            /// </summary>
            HashProgression,

            /// <summary>
            ///     校验失败
            /// </summary>
            HashValidationFailed
        }

        public string AssetName;

        public EventCode Code;

        public ulong DownloadedSize;

        public float Percent;

        public float PercentByFile;
    }
}