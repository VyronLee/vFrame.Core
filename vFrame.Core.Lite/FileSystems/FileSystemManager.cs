using System.Collections.Generic;
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.FileSystems.Adapters;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Package;
using vFrame.Core.FileSystems.Standard;
using vFrame.Core.Loggers;

namespace vFrame.Core.FileSystems
{
    public class FileSystemManager<T> : BaseObject<T> , IFileSystemManager where T: FileStreamFactory
    {
        protected FileStreamFactory _factory;
        private List<IVirtualFileSystem> _fileSystems;

        protected override void OnCreate(T factory) {
            _factory = factory;
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
                if (fileSystem.Exist(path)) {
                    //Logger.Info(FileSystemConst.LogTag, "Get stream: \"{0}\" from file system: \"{1}\"",
                    //    path, fileSystem);
                    return fileSystem.GetStream(path, mode);
                }
            return null;
        }

        public IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(string path) {
            foreach (var fileSystem in _fileSystems)
                if (fileSystem.Exist(path)) {
                    //Logger.Info(FileSystemConst.LogTag, "Get stream async: \"{0}\" from file system: \"{1}\"",
                    //    path, fileSystem);
                    return fileSystem.GetReadonlyStreamAsync(path);
                }
            return null;
        }

        public string ReadAllText(string path) {
            using (var stream = GetStream(path)) {
                return null == stream ? string.Empty : stream.ReadAllText();
            }
        }

        public byte[] ReadAllBytes(string path) {
            using (var stream = GetStream(path)) {
                return stream?.ReadAllBytes();
            }
        }
    }

    public class FileSystemManager : FileSystemManager<FileStreamFactory_Default>
    {
        protected override void OnCreate() {
            base.OnCreate(new FileStreamFactory_Default());
        }
    }
}