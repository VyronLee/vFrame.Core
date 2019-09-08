namespace vFrame.Core.Network
{
    public static class NetworkDef
    {
        // 接收的消息 buffer 大小
        public const int STREAM_BUFF_SIZE = 200 * 1024; // 200KB

        // 发送的消息 buffer 大小
        public const int SEND_STREAM_BUFF_SIZE = 1024; // 1KB

        // Data cache 初始大小
        public const int DATA_CACHE_INIT_SIZE = 10 * 1024; // 10KB

        // 包体长度数据字节数
        public const int DATA_BODY_FLAG_SIZE = 4;

        // 连接超时
        public const int CONNECT_TIME_OUT = 5 * 1000;
    }
}