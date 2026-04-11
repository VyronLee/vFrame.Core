// ------------------------------------------------------------
//         File: ZStdCompressor.cs
//        Brief: Zstandard-based compressor implementation for
//               stream compression and decompression.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-19 11:52:02
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;
using System.IO.Compression;
using vFrame.Core.ThirdParty.ZStd;

namespace vFrame.Core
{
    public class ZStdCompressor : Compressor
    {
        /// <summary>
        /// Compresses data from the input stream to the output stream using Zstandard.
        /// </summary>
        /// <param name="input">The stream containing uncompressed data.</param>
        /// <param name="output">The stream to receive compressed data.</param>
        /// <param name="onProgress">Optional progress callback with (processedBytes, totalBytes).</param>
        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZStdCompressorOptions ?? new ZStdCompressorOptions();
            var totalBytesRead = 0L;
            var inputLength = input.CanSeek ? input.Length : -1;
            using (var encoder = new ZstandardStream(output, CompressionMode.Compress, true)) {
                encoder.CompressionLevel = options.Level;

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
        /// Decompresses data from the input stream to the output stream using Zstandard.
        /// </summary>
        /// <param name="input">The stream containing Zstandard-compressed data.</param>
        /// <param name="output">The stream to receive decompressed data.</param>
        /// <param name="onProgress">Optional progress callback with (processedBytes, totalBytes).</param>
        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = Options as ZStdCompressorOptions ?? new ZStdCompressorOptions();
            var totalBytesRead = 0L;
            var inputLength = input.CanSeek ? input.Length : -1;
            using (var decoder = new ZstandardStream(input, CompressionMode.Decompress, true)) {
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
