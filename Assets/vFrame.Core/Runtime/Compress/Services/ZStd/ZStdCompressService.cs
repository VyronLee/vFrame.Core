using System;
using System.IO;
using System.IO.Compression;
using vFrame.Core.ThirdParty.ZStd;

namespace vFrame.Core.Compress.Services.ZStd
{
    public class ZStdCompressService : CompressService
    {
        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZStdCompressOptions ?? new ZStdCompressOptions();
            using (var encoder = new ZstandardStream(output, CompressionMode.Compress, true)) {
                encoder.CompressionLevel = options.Level;

                int length;
                var buffer = new byte[81920];
                while ((length = input.Read(buffer, 0, buffer.Length)) > 0) {
                    encoder.Write(buffer, 0, length);
                }
            }
        }

        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            using (var decoder = new ZstandardStream(input, CompressionMode.Decompress, true)) {
                int length;
                var buffer = new byte[81920];
                while ((length = decoder.Read(buffer, 0, buffer.Length)) > 0) {
                    output.Write(buffer, 0 ,length);
                }
            }
        }
    }
}