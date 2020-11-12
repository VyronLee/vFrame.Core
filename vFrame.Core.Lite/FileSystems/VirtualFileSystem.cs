using System.Collections.Generic;
using System.IO;
using vFrame.Core.FileSystems.Adapters;

namespace vFrame.Core.FileSystems
{
    public abstract class VirtualFileSystem : IVirtualFileSystem
    {
        protected FileStreamFactory _fileStreamFactory;

        protected VirtualFileSystem(FileStreamFactory streamFactory) {
            _fileStreamFactory = streamFactory;
        }

        public abstract void Open(VFSPath streamVfsPath);

        public abstract void Close();

        public abstract bool Exist(VFSPath relativeVfsPath);

        public abstract IVirtualFileStream GetStream(VFSPath fileName, FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.Read, FileShare share = FileShare.Read);

        public abstract IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(VFSPath fileName);

        public virtual IList<VFSPath> List() {
            return List(new List<VFSPath>());
        }

        public abstract IList<VFSPath> List(IList<VFSPath> refs);
        public abstract event OnGetStreamEventHandler OnGetStream;
    }
}