using System;
using System.IO;

namespace vFrame.Core.Compress.Services
{
    public interface ICompressService
    {
        void Compress(Stream input, Stream output);
        void Compress(Stream input, Stream output, Action<long, long> onProgress);
        void Decompress(Stream input, Stream output);
        void Decompress(Stream input, Stream output, Action<long, long> onProgress);
    }
}