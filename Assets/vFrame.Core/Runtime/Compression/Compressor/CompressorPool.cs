// ------------------------------------------------------------
//         File: CompressorPool.cs
//        Brief: Factory for renting compressor instances
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
    public class CompressorPool : Singleton<CompressorPool>
    {
        /// <summary>
        ///     No-op; compressors are no longer pooled, so there is nothing to
        ///     initialize at singleton creation.
        /// </summary>
        protected override void OnCreate() { }

        /// <summary>
        ///     Rents a new compressor of the specified type. The returned instance
        ///     is owned by the caller and must be disposed (e.g. via <c>using</c>)
        ///     when finished; it is not reused.
        /// </summary>
        /// <param name="compressorType">The type of compressor to rent.</param>
        /// <param name="options">Optional compressor configuration. Uses defaults if null.</param>
        /// <returns>A fresh <see cref="ICompressor" /> instance ready for use.</returns>
        public ICompressor Rent(CompressorType compressorType, CompressorOptions options = null) {
            Compressor compressor = null;
            switch (compressorType) {
                case CompressorType.LZMA:
                    compressor = new LZMACompressor();
                    break;
                case CompressorType.LZ4:
                    compressor = new LZ4Compressor();
                    break;
                case CompressorType.ZStd:
                    compressor = new ZStdCompressor();
                    break;
                case CompressorType.Zlib:
                    compressor = new ZlibCompressor();
                    break;
                default:
                    ThrowHelper.ThrowUnsupportedEnum(compressorType);
                    break;
            }

            compressor?.Create(options);
            return compressor;
        }
    }
}
