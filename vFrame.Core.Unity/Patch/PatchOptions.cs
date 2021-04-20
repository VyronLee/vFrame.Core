namespace vFrame.Core.Patch
{
    public class PatchOptions
    {
        public string storagePath;
        public string hotfixURL;
        public string versionFilename;
        public string manifestFilename;
        public bool deleteCacheOutOfDate;
        public float Timeout = float.MaxValue;
    }
}