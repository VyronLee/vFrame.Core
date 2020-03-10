namespace vFrame.Core.FileSystems.Constants
{
    public static class BlockFlags
    {
        /// 块是否存在
        public static ulong BLOCK_EXISTS = 0x00000001;

        /// 块是否是另一个包文件
        public static ulong BLOCK_ISPACKAGE = 0x00000008;

        /// 块是否是保存为一个整理（可以保存为根据设定大小的连续内存块）
        public static ulong BLOCK_SINGLE_UNIT = 0x00000010;

        /// 块是否进行了压缩（支持叠加压缩）
        public static ulong BLOCK_COMPRESSED = 0x00000F00;

        /// pkware压缩
        public static ulong BLOCK_COMPRESS_PKWARE = 0x00000100;

        /// zlib压缩
        public static ulong BLOCK_COMPRESS_ZLIB = 0x00000200;

        /// huff压缩
        public static ulong BLOCK_COMPRESS_HUFF = 0x00000400;

        /// wave压缩
        public static ulong BLOCK_COMPRESS_WAVE = 0x00000800;

        /// 块是否进行了加密（支持叠加加密）
        public static ulong BLOCK_ENCRYPTED = 0x0000F000;

        /// 加密算法1
        public static ulong BLOCK_ENCRYPT_1 = 0x00001000;

        /// 加密算法2
        public static ulong BLOCK_ENCRYPT_2 = 0x00002000;

        /// 加密算法3
        public static ulong BLOCK_ENCRYPT_3 = 0x00004000;

        /// 加密算法4
        public static ulong BLOCK_ENCRYPT_4 = 0x00008000;

        /// 块的主版本号
        public static ulong BLOCK_VER_MAJOR = 0xFF000000;

        /// 块的从版本号
        public static ulong BLOCK_VER_MINOR = 0x00FF0000;

        /// 块的初始版本号
        public static ulong BLOCK_INIT_VER = 0x01000000;
    }
}