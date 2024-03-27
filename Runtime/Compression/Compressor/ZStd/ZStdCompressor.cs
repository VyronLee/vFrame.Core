using System;
using System.IO;
using System.IO.Compression;
using vFrame.Core.ThirdParty.ZStd;

namespace vFrame.Core.Compression
{
    public class ZStdCompressor : Compressor
    {
        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZStdCompressorOptions ?? new ZStdCompressorOptions();
            using (var encoder = new ZstandardStream(output, CompressionMode.Compress, true)) {
                encoder.CompressionLevel = options.Level;

                int length;
                var buffer = new byte[options.BuffSize];
                while ((length = input.Read(buffer, 0, buffer.Length)) > 0) {
                    encoder.Write(buffer, 0, length);
                }
            }
        }

        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZStdCompressorOptions ?? new ZStdCompressorOptions();
            using (var decoder = new ZstandardStream(input, CompressionMode.Decompress, true)) {
                int length;
                var buffer = new byte[options.BuffSize];
                while ((length = decoder.Read(buffer, 0, buffer.Length)) > 0) {
                    output.Write(buffer, 0, length);
                }
            }
        }
    }
}