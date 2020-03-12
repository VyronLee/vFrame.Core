using System.Collections.Generic;
using System.IO;
using vFrame.Core.FileSystems.Adapters;

namespace vFrame.Core.FileSystems
{
    public abstract class VirtualFileSystem : IVirtualFileSystem
    {
        protected FileStreamFactory FileStreamFactory { get; set; }

        public abstract void Open(VFSPath streamVfsPath);

        public abstract void Close();

        public abstract bool Exist(VFSPath relativeVfsPath);

        public abstract IVirtualFileStream GetStream(VFSPath fileName, FileMode mode, FileAccess access,
            FileShare share);

        public virtual IList<VFSPath> List() {
            return List(new List<VFSPath>());
        }

        public abstract IList<VFSPath> List(IList<VFSPath> refs);
    }
}