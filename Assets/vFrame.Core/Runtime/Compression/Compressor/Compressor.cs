// ------------------------------------------------------------
//         File: Compressor.cs
//        Brief: Abstract base class providing a compress/decompress framework
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
    public abstract class Compressor : BaseObject<CompressorOptions>, ICompressor
    {
        protected CompressorOptions Options { get; private set; }

        /// <summary>
        /// Compresses the input stream to the output stream without progress reporting.
        /// </summary>
        /// <param name="input">The data stream to compress.</param>
        /// <param name="output">The output stream receiving compressed data.</param>
        public virtual void Compress(Stream input, Stream output) {
            Compress(input, output, null);
        }

        /// <summary>
        /// Compresses the input stream to the output stream with progress reporting.
        /// </summary>
        /// <param name="input">The data stream to compress.</param>
        /// <param name="output">The output stream receiving compressed data.</param>
        /// <param name="onProgress">Progress callback with (bytesProcessed, totalBytes).</param>
        public abstract void Compress(Stream input, Stream output, Action<long, long> onProgress);

        /// <summary>
        /// Decompresses the input stream to the output stream without progress reporting.
        /// </summary>
        /// <param name="input">The compressed data stream.</param>
        /// <param name="output">The output stream receiving decompressed data.</param>
        public virtual void Decompress(Stream input, Stream output) {
            Decompress(input, output, null);
        }

        /// <summary>
        /// Decompresses the input stream to the output stream with progress reporting.
        /// </summary>
        /// <param name="input">The compressed data stream.</param>
        /// <param name="output">The output stream receiving decompressed data.</param>
        /// <param name="onProgress">Progress callback with (bytesProcessed, totalBytes).</param>
        public abstract void Decompress(Stream input, Stream output, Action<long, long> onProgress);

        /// <summary>
        /// Stores the compressor options during creation.
        /// </summary>
        /// <param name="options">The compressor configuration options.</param>
        protected override void OnCreate(CompressorOptions options) {
            Options = options;
        }

        /// <summary>
        /// Cleanup logic invoked during destruction.
        /// </summary>
        protected override void OnDestroy() { }
    }
}
