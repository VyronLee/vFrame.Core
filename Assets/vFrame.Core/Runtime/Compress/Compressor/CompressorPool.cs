// ------------------------------------------------------------
//         File: CompressPool.cs
//        Brief: CompressPool.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-18 22:55
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.Exceptions;
using vFrame.Core.ObjectPools;
using vFrame.Core.Singletons;

namespace vFrame.Core.Compress
{
    public class CompressorPool : Singleton<CompressorPool>
    {
        private ObjectPoolManager _poolManager;

        protected override void OnCreate() {
            _poolManager = new ObjectPoolManager();
            _poolManager.Create();
        }

        protected override void OnDestroy() {
            _poolManager?.Destroy();
            _poolManager = null;
        }

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

        public void Return(ICompressor compressor) {
            _poolManager.Return(compressor);
        }
    }

    public class CompressorWrap : BaseObject<CompressorPool, Compressor>, ICompressor
    {
        private CompressorPool _pool;
        private Compressor _compressor;

        protected override void OnCreate(CompressorPool pool, Compressor compressor) {
            _pool = pool;
            _compressor = compressor;
        }

        protected override void OnDestroy() {
            _pool.Return(_compressor);
            _pool.Return(this);
            _pool = null;
            _compressor = null;
        }

        public void Compress(Stream input, Stream output) {
            _compressor.Compress(input, output);
        }

        public void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            _compressor.Compress(input, output, onProgress);
        }

        public void Decompress(Stream input, Stream output) {
            _compressor.Decompress(input, output);
        }

        public void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            _compressor.Decompress(input, output, onProgress);
        }
    }
}