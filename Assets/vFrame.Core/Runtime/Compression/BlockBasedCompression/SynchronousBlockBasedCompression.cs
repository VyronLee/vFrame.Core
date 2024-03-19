using System;
using System.IO;

namespace vFrame.Core.Compression
{
    public class SynchronousBlockBasedCompression : BlockBasedCompression
    {
        public void Compress(Stream input, Stream output, BlockBasedCompressionOptions options, Action<int, int> onProgress = null) {
            BeginCompress(input, output, options);
            for (var i = 0; i < BlockCount; i++) {
                SafeCompress(input, output, options, i);
                onProgress?.Invoke(i, BlockCount);
            }
            EndCompress(output);
        }

        public void Decompress(Stream input, Stream output, Action<int, int> onProgress) {
            BeginDecompress(input, output);
            for (var i = 0; i < BlockCount; i++) {
                SafeDecompress(input, output, i);
                onProgress?.Invoke(i, BlockCount);
            }
            EndDecompress(output);
        }
    }
}