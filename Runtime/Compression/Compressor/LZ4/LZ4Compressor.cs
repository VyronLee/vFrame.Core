using System;
using System.IO;
using K4os.Compression.LZ4.Streams;

namespace vFrame.Core.Compression
{
    public class LZ4Compressor : Compressor
    {
        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as LZ4CompressorOptions ?? new LZ4CompressorOptions();
            using (var encoder = LZ4Stream.Encode(output, options.Level, 0, true)) {
                int length;
                var buffer = new byte[options.BuffSize];
                while ((length = input.Read(buffer, 0, buffer.Length)) > 0) {
                    encoder.Write(buffer, 0, length);
                }
            }
        }

        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            using (var decoder = LZ4Stream.Decode(input, null, true)) {
                int length;
                var buffer = new byte[Options.BuffSize];
                while ((length = decoder.Read(buffer, 0, buffer.Length)) > 0) {
                    output.Write(buffer, 0 ,length);
                }
            }
        }
    }
}