// ------------------------------------------------------------
//         File: SynchronousBlockBasedCompression.cs
//        Brief: Synchronous block-based compression and decompression implementation
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;

namespace vFrame.Core
{
    public class SynchronousBlockBasedCompression : BlockBasedCompression
    {
        /// <summary>
        /// Compresses the input stream synchronously and writes the result to the output stream.
        /// </summary>
        /// <param name="input">The input data stream to compress.</param>
        /// <param name="output">The output stream to receive compressed data.</param>
        /// <param name="options">Compression configuration options.</param>
        /// <param name="onProgress">Optional progress callback receiving (completedBlocks, totalBlocks).</param>
        public void Compress(Stream input, Stream output, BlockBasedCompressionOptions options,
            Action<int, int> onProgress = null) {
            BeginCompress(input, output, options);
            for (var i = 0; i < BlockCount; i++) {
                SafeCompress(input, output, options, i);
                onProgress?.Invoke(i, BlockCount);
            }
            EndCompress(output);
        }

        /// <summary>
        /// Decompresses the input stream synchronously and writes the result to the output stream.
        /// </summary>
        /// <param name="input">The compressed data stream.</param>
        /// <param name="output">The output stream to receive decompressed data.</param>
        /// <param name="onProgress">Optional progress callback receiving (completedBlocks, totalBlocks).</param>
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
