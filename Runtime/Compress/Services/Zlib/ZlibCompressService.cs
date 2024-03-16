using System;
using System.IO;
using vFrame.Core.ThirdParty.Zlib;

namespace vFrame.Core.Compress.Services.Zlib
{
    public class ZlibCompressService : CompressService
    {
        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZlibCompressOptions ?? new ZlibCompressOptions();
            using (var encoder = new DeflateStream(output, CompressionMode.Compress, options.Level, true)) {
                int length;
                var buffer = new byte[81920];
                while ((length = input.Read(buffer, 0, buffer.Length)) > 0) {
                    encoder.Write(buffer, 0, length);
                }
            }
        }

        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZlibCompressOptions ?? new ZlibCompressOptions();
            using (var decoder = new DeflateStream(input, CompressionMode.Decompress, options.Level, true)) {
                int length;
                var buffer = new byte[81920];
                while ((length = decoder.Read(buffer, 0, buffer.Length)) > 0) {
                    output.Write(buffer, 0 ,length);
                }
            }
        }
    }
}