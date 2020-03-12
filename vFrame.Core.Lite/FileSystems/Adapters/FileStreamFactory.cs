using System.IO;

namespace vFrame.Core.FileSystems.Adapters
{
    public abstract class FileStreamFactory
    {
        public abstract FileStreamAdapter Create(string path, FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.Read);
    }
}