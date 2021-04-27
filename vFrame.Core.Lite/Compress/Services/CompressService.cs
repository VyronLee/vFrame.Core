using System;
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.Compress.Services.LZ4;
using vFrame.Core.Compress.Services.LZMA;
using vFrame.Core.Compress.Services.Zlib;
using vFrame.Core.Compress.Services.ZStd;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Compress.Services
{
    public abstract class CompressService : BaseObject<CompressServiceOptions>, ICompressService
    {
        protected CompressServiceOptions Options { get; private set; }

        protected override void OnCreate(CompressServiceOptions options) {
            Options = options;
        }

        protected override void OnDestroy() {
        }

        public virtual void Compress(Stream input, Stream output) {
            Compress(input, output, null);
        }
        public abstract void Compress(Stream input, Stream output, Action<long, long> onProgress);

        public virtual void Decompress(Stream input, Stream output) {
            Decompress(input, output, null);
        }
        public abstract void Decompress(Stream input, Stream output, Action<long, long> onProgress);


        public static CompressService CreateCompressService(CompressType type, CompressServiceOptions options = null) {
            CompressService service;
            switch (type) {
                case CompressType.LZMA:
                    service = ObjectPool<LZMACompressService>.Shared.Get();
                    break;
                case CompressType.LZ4:
                    service = ObjectPool<LZ4CompressService>.Shared.Get();
                    break;
                case CompressType.ZStd:
                    service = ObjectPool<ZStdCompressService>.Shared.Get();
                    break;
                case CompressType.Zlib:
                    service = ObjectPool<ZlibCompressService>.Shared.Get();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            service.Create(options);
            return service;
        }

        public static void DestroyCompressService(CompressService service) {
            service.Destroy();
            switch (service) {
                case LZMACompressService lzmaCompressService:
                    ObjectPool<LZMACompressService>.Shared.Return(lzmaCompressService);
                    break;
                case LZ4CompressService lz4CompressService:
                    ObjectPool<LZ4CompressService>.Shared.Return(lz4CompressService);
                    break;
                case ZStdCompressService zStdCompressService:
                    ObjectPool<ZStdCompressService>.Shared.Return(zStdCompressService);
                    break;
                case ZlibCompressService zlibCompressService:
                    ObjectPool<ZlibCompressService>.Shared.Return(zlibCompressService);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(service.GetType().FullName);
            }
        }
    }
}