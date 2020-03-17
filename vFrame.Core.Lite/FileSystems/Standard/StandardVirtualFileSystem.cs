using System.Collections.Generic;
using System.IO;
using vFrame.Core.FileSystems.Adapters;

namespace vFrame.Core.FileSystems.Standard
{
    internal class StandardVirtualFileSystem : VirtualFileSystem
    {
        private VFSPath _workingDir;

        public StandardVirtualFileSystem() : this(new FileStreamFactory_Default()) {
        }

        public StandardVirtualFileSystem(FileStreamFactory factory) {
            FileStreamFactory = factory;
        }

        public override void Open(VFSPath streamVfsPath) {
            if (!Directory.Exists(streamVfsPath.GetValue())) {
                throw new DirectoryNotFoundException();
            }

            _workingDir = streamVfsPath.AsDirectory();
        }

        public override void Close() {
        }

        public override bool Exist(VFSPath relativeVfsPath) {
            return File.Exists((_workingDir + relativeVfsPath).GetValue());
        }

        public override IVirtualFileStream GetStream(VFSPath fileName,
            FileMode mode,
            FileAccess access,
            FileShare share
        ) {
            var absolutePath = _workingDir + fileName;
            var fileStream = FileStreamFactory.Create(absolutePath.GetValue(), mode, access, share);
            return new StandardVirtualFileStream(fileStream);
        }

        public override IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(VFSPath fileName) {
            if (!Exist(fileName))
                throw new FileNotFoundException("File not found: " + fileName.GetValue());
            var absolutePath = _workingDir + fileName;
            var fileStream = FileStreamFactory.Create(absolutePath.GetValue());
            return new StandardReadonlyVirtualFileStreamRequest(fileStream);
        }

        public override IList<VFSPath> List(IList<VFSPath> refs) {
            var dirInfo = new DirectoryInfo(_workingDir.GetValue());
            TravelDirectory(dirInfo, refs);
            return refs;
        }

        private void TravelDirectory(DirectoryInfo dir, IList<VFSPath> refs) {
            foreach (var fileInfo in dir.GetFiles()) {
                var full = VFSPath.GetPath(fileInfo.FullName);
                var relative = full.GetRelative(_workingDir);
                refs.Add(relative);
            }

            foreach (var subDir in dir.GetDirectories()) {
                TravelDirectory(subDir, refs);
            }
        }

        public override string ToString() {
            return _workingDir.GetValue();
        }
    }
}