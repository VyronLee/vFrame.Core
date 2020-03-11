using System.Collections.Generic;
using System.IO;
using vFrame.Core.FileSystems.Adapters;

namespace vFrame.Core.FileSystems
{
    public abstract class FileSystem : IFileSystem
    {
        protected FileStreamFactory FileStreamFactory { get; set; }

        public abstract void Open(Path streamPath);

        public abstract void Close();

        public abstract bool Exist(Path relativePath);

        public Stream GetStream(Path fileName) {
            return GetStream(fileName, FileMode.Open);
        }

        public Stream GetStream(Path fileName, FileMode mode) {
            return GetStream(fileName, mode, FileAccess.ReadWrite);
        }

        public Stream GetStream(Path fileName, FileMode mode, FileAccess access) {
            return GetStream(fileName, mode, access, FileShare.Read);
        }

        public abstract Stream GetStream(Path fileName, FileMode mode, FileAccess access, FileShare share);

        public virtual IList<Path> List() {
            return List(new List<Path>());
        }

        public abstract IList<Path> List(IList<Path> refs);
    }
}