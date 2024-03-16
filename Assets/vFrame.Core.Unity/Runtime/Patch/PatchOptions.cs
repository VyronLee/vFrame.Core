namespace vFrame.Core.Patch
{
    public class PatchOptions
    {
        public string cdnUrl;
        public string cdnDir;
        public string versionUrl;
        public string storagePath;
        public string versionFilename;
        public string manifestFilename;
        public bool deleteCacheOutOfDate;
        public int timeout = int.MaxValue;
    }
}