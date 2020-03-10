using System;
using System.Collections.Generic;
using System.IO;

namespace vFrame.Core.FileSystems
{
    public abstract class FileSystem : IFileSystem
    {
        public abstract bool Open(string streamPath);

        public abstract bool Close();

        public abstract bool Exist(string relativePath);

        public abstract Stream GetStream(string fileName, FileMode mode = FileMode.Open);


        public virtual IList<Path> List() {
            return List(new List<Path>());
        }

        public abstract IList<Path> List(IList<Path> refs);
    }
}