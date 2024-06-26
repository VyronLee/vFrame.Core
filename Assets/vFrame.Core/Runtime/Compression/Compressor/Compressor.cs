﻿using System;
using System.IO;
using vFrame.Core.Base;

namespace vFrame.Core.Compression
{
    public abstract class Compressor : BaseObject<CompressorOptions>, ICompressor
    {
        protected CompressorOptions Options { get; private set; }

        public virtual void Compress(Stream input, Stream output) {
            Compress(input, output, null);
        }

        public abstract void Compress(Stream input, Stream output, Action<long, long> onProgress);

        public virtual void Decompress(Stream input, Stream output) {
            Decompress(input, output, null);
        }

        public abstract void Decompress(Stream input, Stream output, Action<long, long> onProgress);

        protected override void OnCreate(CompressorOptions options) {
            Options = options;
        }

        protected override void OnDestroy() { }
    }
}