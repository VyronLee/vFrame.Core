using System.Collections.Generic;
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Package;
using vFrame.Core.FileSystems.Standard;

namespace vFrame.Core.FileSystems
{
    public class FileSystemManager : BaseObject , IFileSystemManager
    {
        private List<IVirtualFileSystem> _fileSystems;

        protected override void OnCreate() {
            _fileSystems = new List<IVirtualFileSystem>();
        }

        protected override void OnDestroy() {
            foreach (var fileSystem in _fileSystems)
                fileSystem.Close();
            _fileSystems.Clear();
        }

        public virtual IVirtualFileSystem AddFileSystem(VFSPath vfsPath) {
            IVirtualFileSystem virtualFileSystem;
            switch (vfsPath.GetExtension()) {
                case PackageFileSystemConst.Ext:
                    virtualFileSystem = new PackageVirtualFileSystem();
                    break;
                default:
                    virtualFileSystem = new StandardVirtualFileSystem();
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

        public IEnumerator<IVirtualFileSystem> GetEnumerator() {
            foreach (var fileSystem in _fileSystems) {
                yield return fileSystem;
            }
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
}