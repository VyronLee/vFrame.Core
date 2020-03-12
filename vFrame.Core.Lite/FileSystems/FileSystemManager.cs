using System.Collections.Generic;
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.FileSystems.Adapters;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Package;
using vFrame.Core.FileSystems.Standard;

namespace vFrame.Core.FileSystems
{
    public class FileSystemManager : BaseObject<FileStreamFactory>
    {
        private FileStreamFactory _factory;
        private List<IVirtualFileSystem> _fileSystems;

        protected override void OnCreate(FileStreamFactory factory) {
            _factory = factory ?? new FileStreamFactory_Default();
            _fileSystems = new List<IVirtualFileSystem>();
        }

        protected override void OnDestroy() {
            foreach (var fileSystem in _fileSystems)
                fileSystem.Close();
            _fileSystems.Clear();
        }

        public IVirtualFileSystem AddFileSystem(VFSPath vfsPath) {
            IVirtualFileSystem virtualFileSystem;
            switch (vfsPath.GetExtension()) {
                case PackageFileSystemConst.Ext:
                    virtualFileSystem = new PackageVirtualFileSystem(_factory);
                    break;
                default:
                    virtualFileSystem = new StandardVirtualFileSystem(_factory);
                    break;
            }

            virtualFileSystem.Open(vfsPath);

            AddFileSystem(virtualFileSystem);
            return virtualFileSystem;
        }

        public void AddFileSystem(IVirtualFileSystem virtualFileSystem) {
            _fileSystems.Add(virtualFileSystem);
        }

        public void RemoveFileSystem(IVirtualFileSystem virtualFileSystem) {
            _fileSystems.Remove(virtualFileSystem);
        }

        public IVirtualFileStream GetStream(string path, FileMode mode = FileMode.Open) {
            foreach (var fileSystem in _fileSystems)
                if (fileSystem.Exist(path))
                    return fileSystem.GetStream(path, mode);
            return null;
        }
    }
}