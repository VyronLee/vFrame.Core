using System.IO;

namespace vFrame.Core.FileSystems.Adapters
{
    public abstract class FileStreamFactory
    {
        public abstract FileStreamAdapter Create(string path, FileMode mode, FileAccess access, FileShare share);

        public FileStreamAdapter Create(string path, FileMode mode, FileAccess access) {
            return Create(path, mode, FileAccess.ReadWrite);
        }

        public FileStreamAdapter Create(string path, FileMode mode) {
            return Create(path, mode, FileAccess.ReadWrite, FileShare.Read);
        }

        public FileStreamAdapter Create(string path) {
            return Create(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        }
    }
}