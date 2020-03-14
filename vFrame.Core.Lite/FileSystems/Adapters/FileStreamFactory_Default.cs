using System.IO;

namespace vFrame.Core.FileSystems.Adapters
{
    public class FileStreamFactory_Default : FileStreamFactory
    {
        public override FileStreamAdapter Create(string path,
            FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read
        ) {
            return new FileStreamAdapter_Default(path, mode, access, share);
        }
    }
}