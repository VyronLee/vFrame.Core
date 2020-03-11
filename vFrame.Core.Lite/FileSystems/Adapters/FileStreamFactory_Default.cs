using System.IO;

namespace vFrame.Core.FileSystems.Adapters
{
    internal class FileStreamFactory_Default : FileStreamFactory
    {
        public override FileStreamAdapter Create(string path, FileMode mode, FileAccess access, FileShare share) {
            return new FileStreamAdapter_Default(path, mode, access, share);
        }
    }
}