// ------------------------------------------------------------
//         File: LZ4Compressor.cs
//        Brief: LZ4 algorithm-based compressor implementation
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;
using K4os.Compression.LZ4.Streams;

namespace vFrame.Core
{
    public class LZ4Compressor : Compressor
    {
        /// <summary>
        ///     Compresses the input stream using the LZ4 algorithm.
        /// </summary>
        /// <param name="input">The stream containing data to compress.</param>
        /// <param name="output">The stream to write compressed data to.</param>
        /// <param name="onProgress">Progress callback (currently unused).</param>
        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as LZ4CompressorOptions ?? new LZ4CompressorOptions();
            var totalBytesRead = 0L;
            var inputLength = input.CanSeek ? input.Length : -1;
            using (var encoder = LZ4Stream.Encode(output, options.Level, 0, true)) {
                int length;
                var buffer = new byte[options.BuffSize];
                while ((length = input.Read(buffer, 0, buffer.Length)) > 0) {
                    encoder.Write(buffer, 0, length);
                    totalBytesRead += length;
                    onProgress?.Invoke(totalBytesRead, inputLength);
                }
            }
        }

        /// <summary>
        ///     Decompresses the LZ4-compressed input stream.
        /// </summary>
        /// <param name="input">The stream containing LZ4-compressed data.</param>
        /// <param name="output">The stream to write decompressed data to.</param>
        /// <param name="onProgress">Progress callback (currently unused).</param>
        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as LZ4CompressorOptions ?? new LZ4CompressorOptions();
            var totalBytesRead = 0L;
            var inputLength = input.CanSeek ? input.Length : -1;
            using (var decoder = LZ4Stream.Decode(input, null, true)) {
                int length;
                var buffer = new byte[options.BuffSize];
                while ((length = decoder.Read(buffer, 0, buffer.Length)) > 0) {
                    output.Write(buffer, 0, length);
                    totalBytesRead += length;
                    onProgress?.Invoke(totalBytesRead, inputLength);
                }
            }
        }
    }
}