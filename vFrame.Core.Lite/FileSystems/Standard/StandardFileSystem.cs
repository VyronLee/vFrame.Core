using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace vFrame.Core.FileSystems.Standard
{
    public class StandardFileSystem : FileSystem
    {
        private Path _workingDir;

        public override bool Open(string streamPath) {
            _workingDir = new Path(streamPath);
            Debug.Assert(_workingDir.IsAbsolute());
            return true;
        }

        public override bool Close() {
            return true;
        }

        public override bool Exist(string relativePath) {
            return File.Exists((_workingDir + relativePath).GetValue());
        }

        public override Stream GetStream(string fileName, FileMode mode = FileMode.Open) {
            if (!Exist(fileName)) {
                throw new FileNotFoundException("File not found: " + fileName);
            }
            return new FileStream(fileName, mode);
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