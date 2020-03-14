using System.IO;

namespace vFrame.Core.FileSystems
{
    public abstract class VirtualFileStream : Stream, IVirtualFileStream
    {
        public string ReadAllText() {
            using (var reader = new StreamReader(this)) {
                return reader.ReadToEnd();
            }
        }

        public byte[] ReadAllBytes() {
            using (var reader = new BinaryReader(this)) {
                return reader.ReadBytes((int) Length);
            }
        }
    }
}