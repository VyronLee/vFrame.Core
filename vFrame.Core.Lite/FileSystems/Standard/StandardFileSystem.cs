using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using vFrame.Core.FileSystems.Adapters;

namespace vFrame.Core.FileSystems.Standard
{
    public class StandardFileSystem : FileSystem
    {
        private Path _workingDir;

        public StandardFileSystem(FileStreamFactory factory) {
            FileStreamFactory = factory;
        }

        public override void Open(Path streamPath) {
            Debug.Assert(streamPath.IsAbsolute());
            _workingDir = streamPath;
        }

        public override void Close() {
        }

        public override bool Exist(Path relativePath) {
            return File.Exists((_workingDir + relativePath).GetValue());
        }

        public override Stream GetStream(Path fileName, FileMode mode, FileAccess access, FileShare share) {
            if (!Exist(fileName)) throw new FileNotFoundException("File not found: " + fileName.GetValue());
            return FileStreamFactory.Create(fileName.GetValue(), mode, access, share);
        }

        public override IList<Path> List(IList<Path> refs) {
            var dirInfo = new DirectoryInfo(_workingDir.GetValue());
            foreach (var fileInfo in dirInfo.GetFiles()) {
                var full = Path.GetPath(fileInfo.FullName);
                var relative = full.GetRelative(_workingDir);
                refs.Add(relative);
            }

            return refs;
        }
    }
}