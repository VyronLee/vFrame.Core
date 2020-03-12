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
        private List<IFileSystem> _fileSystems;
        private FileStreamFactory _factory;

        protected override void OnCreate(FileStreamFactory factory) {
            _factory = factory ?? new FileStreamFactory_Default();
            _fileSystems = new List<IFileSystem>();
        }

        protected override void OnDestroy() {
            foreach (var fileSystem in _fileSystems) fileSystem.Close();
            _fileSystems.Clear();
        }

        public IFileSystem AddFileSystem(Path path) {
            IFileSystem fileSystem;
            switch (path.GetExtension()) {
                case PackageFileSystemConst.Ext:
                    fileSystem = new PackageFileSystem(_factory);
                    break;
                default:
                    fileSystem = new StandardFileSystem(_factory);
                    break;
            }

            fileSystem.Open(path);

            AddFileSystem(fileSystem);
            return fileSystem;
        }

        public void AddFileSystem(IFileSystem fileSystem) {
            _fileSystems.Add(fileSystem);
        }

        public void RemoveFileSystem(IFileSystem fileSystem) {
            _fileSystems.Remove(fileSystem);
        }

        public Stream GetStream(string path, FileMode mode = FileMode.Open) {
            foreach (var fileSystem in _fileSystems)
                if (fileSystem.Exist(path))
                    return fileSystem.GetStream(path, mode);
            return null;
        }
    }
}