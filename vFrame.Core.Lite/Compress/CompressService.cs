using System;
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.Compress.LZMA;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Compress
{
    public abstract class CompressService : BaseObject<CompressServiceOptions>, ICompressService
    {
        protected CompressServiceOptions Options { get; private set; }

        protected override void OnCreate(CompressServiceOptions options) {
            Options = options;
        }

        protected override void OnDestroy() {
        }

        public abstract void Compress(Stream input, Stream output);
        public abstract void Decompress(Stream input, Stream output);


        public static CompressService CreateCompressService(CompressType type, CompressServiceOptions options = null) {
            CompressService service;
            switch (type) {
                case CompressType.LZMA:
                    service = ObjectPool<LZMACompressService>.Shared.Get();
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
                default:
                    throw new ArgumentOutOfRangeException(service.GetType().FullName);
            }
        }
    }
}