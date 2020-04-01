namespace vFrame.Core.Patch
{
    public class UpdateEvent
    {
        public enum EventCode
        {
            /// <summary>
            /// 找不到本地配置
            /// </summary>
            ERROR_NO_LOCAL_MANIFEST,

            /// <summary>
            /// 下载版本号失败
            /// </summary>
            ERROR_DOWNLOAD_VERSION,

            /// <summary>
            /// 解析版本号失败
            /// </summary>
            ERROR_PARSE_VERSION,

            /// <summary>
            /// 下载manifest失败
            /// </summary>
            ERROR_DOWNLOAD_MANIFEST,

            /// <summary>
            /// 解析manifest失败
            /// </summary>
            ERROR_PARSE_MANIFEST,

            /// <summary>
            /// 发现新版本
            /// </summary>
            NEW_ASSETS_VERSION_FOUND,

            /// <summary>
            /// 发现新版本
            /// </summary>
            NEW_GAME_VERSION_FOUND,

            /// <summary>
            /// 已最新
            /// </summary>
            ALREADY_UP_TO_DATE,

            /// <summary>
            /// 更新进度
            /// </summary>
            UPDATE_PROGRESSION,

            /// <summary>
            /// 单个资源下载成功
            /// </summary>
            ASSET_UPDATED,

            /// <summary>
            /// 单个资源下载失败
            /// </summary>
            ERROR_UPDATING,

            /// <summary>
            /// 更新成功
            /// </summary>
            UPDATE_FINISHED,

            /// <summary>
            /// 更新失败
            /// </summary>
            UPDATE_FAILED,

            /// <summary>
            /// 哈希检测
            /// </summary>
            HASH_START,

            /// <summary>
            /// 哈希进度
            /// </summary>
            HASH_PROGRESSION,
        }

        public EventCode Code;

        public string AssetName;

        public float PercentByFile;

        public float Percent;

        public ulong DownloadedSize;

        public string DownloadSpeed;
    }
}