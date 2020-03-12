using System.Collections.Generic;
using System.IO;
using vFrame.Core.FileSystems.Adapters;

namespace vFrame.Core.FileSystems.Standard
{
    internal class StandardVirtualFileSystem : VirtualFileSystem
    {
        private VFSPath _workingDir;

        public StandardVirtualFileSystem(FileStreamFactory factory) {
            FileStreamFactory = factory;
        }

        public override void Open(VFSPath streamVfsPath) {
            _workingDir = streamVfsPath.AsDirectory();
        }

        public override void Close() {
        }

        public override bool Exist(VFSPath relativeVfsPath) {
            return File.Exists((_workingDir + relativeVfsPath).GetValue());
        }

        public override IVirtualFileStream GetStream(VFSPath fileName, FileMode mode, FileAccess access,
            FileShare share) {
            if (!Exist(fileName))
                throw new FileNotFoundException("File not found: " + fileName.GetValue());
            var fileStream = FileStreamFactory.Create(fileName.GetValue(), mode, access, share);
            return new StandardVirtualFileStream(fileStream);
        }

        public override IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(VFSPath fileName) {
            if (!Exist(fileName))
                throw new FileNotFoundException("File not found: " + fileName.GetValue());
            var fileStream = FileStreamFactory.Create(fileName.GetValue());
            return new StandardReadonlyVirtualFileStreamRequest(fileStream);
        }

        public override IList<VFSPath> List(IList<VFSPath> refs) {
            var dirInfo = new DirectoryInfo(_workingDir.GetValue());
            foreach (var fileInfo in dirInfo.GetFiles()) {
                var full = VFSPath.GetPath(fileInfo.FullName);
                var relative = full.GetRelative(_workingDir);
                refs.Add(relative);
            }

            return refs;
        }
    }
}