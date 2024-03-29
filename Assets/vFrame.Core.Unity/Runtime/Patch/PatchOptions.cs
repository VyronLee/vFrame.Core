namespace vFrame.Core.Unity.Patch
{
    public class PatchOptions
    {
        public string cdnDir;
        public string cdnUrl;
        public bool deleteCacheOutOfDate;
        public string manifestFilename;
        public string storagePath;
        public int timeout = int.MaxValue;
        public string versionFilename;
        public string versionUrl;
    }
}