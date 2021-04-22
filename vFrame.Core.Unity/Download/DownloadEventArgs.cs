namespace vFrame.Core.Download
{
    public struct DownloadEventArgs
    {
        public int SerialId { get; set; }

        public string Error { get; set; }

        public ulong DownloadedSize { get; set; }

        public ulong TotalSize { get; set; }

        public float Progress { get; set; }

        public object UserData { get; set; }
    }
}