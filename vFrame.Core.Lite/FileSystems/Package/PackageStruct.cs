namespace vFrame.Core.FileSystems.Package
{
    internal struct PackageHeader
    {
        public long Id;
        public long Version;
        public long Size;
        public long FileListOffset;
        public long FileListSize;
        public long BlockTableOffset;
        public long BlockTableSize;
        public long BlockOffset;
        public long Reserved;
    }

    internal struct PackageBlock
    {
        public long Flags;
        public long Offset;
        public long OriginalSize;
        public long CompressedSize;
        public long Seed;
    }
}