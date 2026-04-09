// ------------------------------------------------------------
//         File: ZlibCompressor.cs
//        Brief: Zlib-based compressor implementation for stream
//               compression and decompression.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-19 11:52:02
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;
using vFrame.Core.ThirdParty.Zlib;

namespace vFrame.Core
{
    public class ZlibCompressor : Compressor
    {
        /// <summary>
        /// Compresses data from the input stream to the output stream using Zlib deflate.
        /// </summary>
        /// <param name="input">The stream containing uncompressed data.</param>
        /// <param name="output">The stream to receive compressed data.</param>
        /// <param name="onProgress">Optional progress callback with (processedBytes, totalBytes).</param>
        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZlibCompressorOptions ?? new ZlibCompressorOptions();
            using (var encoder = new DeflateStream(output, CompressionMode.Compress, options.Level, true)) {
                int length;
                var buffer = new byte[options.BuffSize];
                while ((length = input.Read(buffer, 0, buffer.Length)) > 0) {
                    encoder.Write(buffer, 0, length);
                }
            }
        }

        /// <summary>
        /// Decompresses data from the input stream to the output stream using Zlib inflate.
        /// </summary>
        /// <param name="input">The stream containing Zlib-compressed data.</param>
        /// <param name="output">The stream to receive decompressed data.</param>
        /// <param name="onProgress">Optional progress callback with (processedBytes, totalBytes).</param>
        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZlibCompressorOptions ?? new ZlibCompressorOptions();
            using (var decoder = new DeflateStream(input, CompressionMode.Decompress, options.Level, true)) {
                int length;
                var buffer = new byte[options.BuffSize];
                while ((length = decoder.Read(buffer, 0, buffer.Length)) > 0) {
                    output.Write(buffer, 0, length);
                }
            }
        }
    }
}
