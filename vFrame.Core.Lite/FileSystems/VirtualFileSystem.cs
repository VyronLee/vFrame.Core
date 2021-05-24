using System.Collections.Generic;
using System.IO;

namespace vFrame.Core.FileSystems
{
    public abstract class VirtualFileSystem : IVirtualFileSystem
    {
        protected VirtualFileSystem() {

        }

        public abstract void Open(VFSPath fsPath);
        public abstract void Open(Stream stream);

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

        public void Dispose() {
            Close();
        }
    }
}