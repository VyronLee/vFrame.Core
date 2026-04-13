// ------------------------------------------------------------
//         File: CompressorPool.cs
//        Brief: Object pool for renting and returning compressor instances
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
        private ObjectPoolManager _poolManager;

        /// <summary>
        ///     Initializes the pool manager during creation.
        /// </summary>
        protected override void OnCreate() {
            _poolManager = new ObjectPoolManager();
            _poolManager.Create();
        }

        /// <summary>
        ///     Tears down the pool manager during destruction.
        /// </summary>
        protected override void OnDestroy() {
            _poolManager?.Destroy();
            _poolManager = null;
        }

        /// <summary>
        ///     Rents a compressor of the specified type, wrapped for automatic return.
        /// </summary>
        /// <param name="compressorType">The type of compressor to rent.</param>
        /// <param name="options">Optional compressor configuration. Uses defaults if null.</param>
        /// <returns>An <see cref="ICompressor" /> that returns itself to the pool on dispose.</returns>
        public ICompressor Rent(CompressorType compressorType, CompressorOptions options = null) {
            Compressor compressor = null;
            switch (compressorType) {
                case CompressorType.LZMA:
                    compressor = _poolManager.GetObjectPool<LZMACompressor>().Get();
                    break;
                case CompressorType.LZ4:
                    compressor = _poolManager.GetObjectPool<LZ4Compressor>().Get();
                    break;
                case CompressorType.ZStd:
                    compressor = _poolManager.GetObjectPool<ZStdCompressor>().Get();
                    break;
                case CompressorType.Zlib:
                    compressor = _poolManager.GetObjectPool<ZlibCompressor>().Get();
                    break;
                default:
                    ThrowHelper.ThrowUnsupportedEnum(compressorType);
                    break;
            }

            compressor?.Create(options);

            var wrap = _poolManager.GetObjectPool<CompressorWrap>().Get();
            wrap.Create(this, compressor);
            return wrap;
        }

        /// <summary>
        ///     Returns a compressor or wrapper to its pool.
        /// </summary>
        /// <param name="compressor">The compressor instance to return.</param>
        public void Return(ICompressor compressor) {
            _poolManager.Return(compressor);
        }
    }

    /// <summary>
    ///     Poolable wrapper that delegates compression calls to an inner <see cref="Compressor" />
    ///     and returns both the inner instance and itself to the pool on destruction.
    /// </summary>
    public class CompressorWrap : BaseObject<CompressorPool, Compressor>, ICompressor
    {
        private Compressor _compressor;
        private CompressorPool _pool;

        /// <summary>
        ///     Compresses the input stream to the output stream.
        /// </summary>
        /// <param name="input">The data stream to compress.</param>
        /// <param name="output">The output stream receiving compressed data.</param>
        public void Compress(Stream input, Stream output) {
            _compressor.Compress(input, output);
        }

        /// <summary>
        ///     Compresses the input stream to the output stream with progress reporting.
        /// </summary>
        /// <param name="input">The data stream to compress.</param>
        /// <param name="output">The output stream receiving compressed data.</param>
        /// <param name="onProgress">Progress callback with (bytesProcessed, totalBytes).</param>
        public void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            _compressor.Compress(input, output, onProgress);
        }

        /// <summary>
        ///     Decompresses the input stream to the output stream.
        /// </summary>
        /// <param name="input">The compressed data stream.</param>
        /// <param name="output">The output stream receiving decompressed data.</param>
        public void Decompress(Stream input, Stream output) {
            _compressor.Decompress(input, output);
        }

        /// <summary>
        ///     Decompresses the input stream to the output stream with progress reporting.
        /// </summary>
        /// <param name="input">The compressed data stream.</param>
        /// <param name="output">The output stream receiving decompressed data.</param>
        /// <param name="onProgress">Progress callback with (bytesProcessed, totalBytes).</param>
        public void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            _compressor.Decompress(input, output, onProgress);
        }

        /// <summary>
        ///     Stores the pool reference and inner compressor during creation.
        /// </summary>
        /// <param name="pool">The owning compressor pool.</param>
        /// <param name="compressor">The inner compressor instance to delegate to.</param>
        protected override void OnCreate(CompressorPool pool, Compressor compressor) {
            _pool = pool;
            _compressor = compressor;
        }

        /// <summary>
        ///     Returns the inner compressor and this wrapper to the pool during destruction.
        /// </summary>
        protected override void OnDestroy() {
            _pool.Return(_compressor);
            _pool.Return(this);
            _pool = null;
            _compressor = null;
        }
    }
}