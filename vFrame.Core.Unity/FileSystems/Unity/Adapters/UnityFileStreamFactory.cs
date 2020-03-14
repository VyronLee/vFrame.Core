using System.IO;
using vFrame.Core.FileSystems.Adapters;

namespace vFrame.Core.FileSystems.Unity.Adapters
{
    public class UnityFileStreamFactory : FileStreamFactory
    {
        public override FileStreamAdapter Create(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.Read) {
            return new UnityFileStreamAdapter(path, mode, access, share);
        }
    }
}