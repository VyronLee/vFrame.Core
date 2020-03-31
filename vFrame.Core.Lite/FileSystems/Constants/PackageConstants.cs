using vFrame.Core.Loggers;

namespace vFrame.Core.FileSystems.Constants
{
    public static class FileSystemConst
    {
        // 虚拟文件系统LogTag
        public static readonly LogTag LogTag = new LogTag("VirtualFileSystem");
    }

    public static class PackageFileSystemConst
    {
        // 包文件标识
        public const long Id = 0x737172757368; // sqrush

        // 包文件版本号
        public const long Version = 0x1;

        // 包文件后缀
        public const string Ext = ".vpk";

        // ReSharper disable once CommentTypo
        // 文件列表加密密钥
        public const long FileListEncryptKey = 0x7368656e7175616e; // shenquan
    }

    public static class BlockFlags
    {
        /// 块是否存在
        public const long BlockExists = 0x00000001;

        /// 块是否进行了压缩
        public const long BlockCompressed = 0x00000F00;

        /// 压缩算法LZMA
        public const long BlockCompressZlib = 0x00000100;

        /// 块是否进行了加密
        public const long BlockEncrypted = 0x0000F000;

        /// 加密算法XOR
        public const long BlockEncryptXor = 0x00001000;

        /// 加密算法AES
        public const long BlockEncryptAes = 0x00002000;

        /// 块的主版本号
        public const long BlockVerMajor = 0xFF000000;

        /// 块的从版本号
        public const long BlockVerMinor = 0x00FF0000;

        /// 块的初始版本号
        public const long BlockInitVer = 0x01000000;
    }
}