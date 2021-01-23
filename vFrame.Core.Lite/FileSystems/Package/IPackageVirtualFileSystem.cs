namespace vFrame.Core.FileSystems.Package
{
    public interface IPackageVirtualFileSystem : IVirtualFileSystem
    {
        VFSPath PackageFilePath { get; }
        PackageBlockInfo GetBlockInfo(string fileName);
        event OnGetPackageBlockEventHandler OnGetBlock;
    }

    public delegate void OnGetPackageBlockEventHandler(string vfsPath, string filePath);
}