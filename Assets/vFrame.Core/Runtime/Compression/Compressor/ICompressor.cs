// ------------------------------------------------------------
//         File: ICompressor.cs
//        Brief: Defines the common interface for compressors
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
    /// <summary>
    /// Provides compression and decompression operations on streams.
    /// </summary>
    public interface ICompressor : IDisposable
    {
        /// <summary>
        /// Compresses the input stream to the output stream.
        /// </summary>
        /// <param name="input">The data stream to compress.</param>
        /// <param name="output">The output stream receiving compressed data.</param>
        void Compress(Stream input, Stream output);

        /// <summary>
        /// Compresses the input stream to the output stream with progress reporting.
        /// </summary>
        /// <param name="input">The data stream to compress.</param>
        /// <param name="output">The output stream receiving compressed data.</param>
        /// <param name="onProgress">Progress callback with (bytesProcessed, totalBytes).</param>
        void Compress(Stream input, Stream output, Action<long, long> onProgress);

        /// <summary>
        /// Decompresses the input stream to the output stream.
        /// </summary>
        /// <param name="input">The compressed data stream.</param>
        /// <param name="output">The output stream receiving decompressed data.</param>
        void Decompress(Stream input, Stream output);

        /// <summary>
        /// Decompresses the input stream to the output stream with progress reporting.
        /// </summary>
        /// <param name="output">The output stream receiving decompressed data.</param>
        /// <param name="input">The compressed data stream.</param>
        /// <param name="onProgress">Progress callback with (bytesProcessed, totalBytes).</param>
        void Decompress(Stream input, Stream output, Action<long, long> onProgress);
    }
}
