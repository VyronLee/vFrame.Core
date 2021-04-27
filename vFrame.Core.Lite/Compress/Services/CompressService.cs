﻿using System;
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.Compress.Services.LZMA;
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