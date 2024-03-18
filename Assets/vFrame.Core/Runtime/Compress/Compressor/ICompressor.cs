using System;
using System.IO;

namespace vFrame.Core.Compress
{
    public interface ICompressor : IDisposable
    {
        void Compress(Stream input, Stream output);
        void Compress(Stream input, Stream output, Action<long, long> onProgress);
        void Decompress(Stream input, Stream output);
        void Decompress(Stream input, Stream output, Action<long, long> onProgress);
    }
}