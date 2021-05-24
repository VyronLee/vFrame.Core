using System.IO;

namespace vFrame.Core.FileSystems.Package
{
    public interface IPackageVirtualFileSystem : IVirtualFileSystem
    {
        VFSPath PackageFilePath { get; }
        PackageBlockInfo GetBlockInfo(string fileName);
        void AddFile(string filePath);
        void AddFile(string filePath, long encryptType, long encryptKey, long compressType);
        void AddStream(string filePath, Stream stream);
        void AddStream(string filePath, Stream stream, long encryptType, long encryptKey, long compressType);
        void DeleteFile(string filePath);
        void Flush();
        bool ReadOnly { get; }
        event OnGetPackageBlockEventHandler OnGetBlock;
    }

    public delegate void OnGetPackageBlockEventHandler(string vfsPath, string filePath);
}