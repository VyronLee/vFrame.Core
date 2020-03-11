using System.IO;

namespace vFrame.Core.Compress
{
    public interface ICompressService
    {
        void Compress(Stream input, Stream output);
        void Decompress(Stream input, Stream output);
    }
}