using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.ObjectPools;
using vFrame.Core.ThirdParty.SevenZip;
using vFrame.Core.ThirdParty.SevenZip.Compression.LZMA;

namespace vFrame.Core.Compress.Services.LZMA
{
    public class LZMACompressService : CompressService
    {
        private static readonly LZMACompressOptions DefaultOptions = new LZMACompressOptions() {
            DictionarySize = LZMACompressOptions.LZMADictionarySize.Small,
            Speed = LZMACompressOptions.LZMASpeed.Medium,
        };

        private static readonly Dictionary<LZMACompressOptions.LZMADictionarySize, ConcurrentQueue<Encoder>> EncoderCache
            = new Dictionary<LZMACompressOptions.LZMADictionarySize, ConcurrentQueue<Encoder>> {
                {LZMACompressOptions.LZMADictionarySize.VerySmall, new ConcurrentQueue<Encoder>()},
                {LZMACompressOptions.LZMADictionarySize.Small, new ConcurrentQueue<Encoder>()},
                {LZMACompressOptions.LZMADictionarySize.Medium, new ConcurrentQueue<Encoder>()},
                {LZMACompressOptions.LZMADictionarySize.Large, new ConcurrentQueue<Encoder>()},
                {LZMACompressOptions.LZMADictionarySize.Larger, new ConcurrentQueue<Encoder>()},
                {LZMACompressOptions.LZMADictionarySize.VeryLarge, new ConcurrentQueue<Encoder>()},
            };

        private static readonly Dictionary<LZMACompressOptions.LZMADictionarySize, ConcurrentQueue<Decoder>> DecoderCache
            = new Dictionary<LZMACompressOptions.LZMADictionarySize, ConcurrentQueue<Decoder>> {
                {LZMACompressOptions.LZMADictionarySize.VerySmall, new ConcurrentQueue<Decoder>()},
                {LZMACompressOptions.LZMADictionarySize.Small, new ConcurrentQueue<Decoder>()},
                {LZMACompressOptions.LZMADictionarySize.Medium, new ConcurrentQueue<Decoder>()},
                {LZMACompressOptions.LZMADictionarySize.Large, new ConcurrentQueue<Decoder>()},
                {LZMACompressOptions.LZMADictionarySize.Larger, new ConcurrentQueue<Decoder>()},
                {LZMACompressOptions.LZMADictionarySize.VeryLarge, new ConcurrentQueue<Decoder>()},
            };

        private static readonly ConcurrentQueue<object[]> ObjectArrayCache = new ConcurrentQueue<object[]>();
        private static readonly ConcurrentQueue<byte[]> ByteArrayCache = new ConcurrentQueue<byte[]>();

        private static readonly CoderPropID[] PropIDs = {
            CoderPropID.DictionarySize,
            CoderPropID.PosStateBits, // (0 <= x <= 4).
            CoderPropID.LitContextBits, // (0 <= x <= 8).
            CoderPropID.LitPosBits, // (0 <= x <= 4).
            CoderPropID.NumFastBytes,
            CoderPropID.MatchFinder, // "BT2", "BT4".
            CoderPropID.EndMarker
        };

        protected override void OnCreate(CompressServiceOptions options) {
            base.OnCreate(options ?? DefaultOptions);
        }

        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = (LZMACompressOptions) Options;

            const int posStateBits = 2; // default: 2
            const int litContextBits = 3; // 3 for normal files, 0; for 32-bit data
            const int litPosBits = 0; // 0 for 64-bit data, 2 for 32-bit.
            const string matchFinder = "BT4"; // default: BT4
            const bool endMarker = true;

            if (!ObjectArrayCache.TryDequeue(out var properties)) {
                properties = new object[7];
            }

            properties[0] = (int) options.DictionarySize;
            properties[1] = posStateBits;
            properties[2] = litContextBits;
            properties[3] = litPosBits;
            properties[4] = (int) options.Speed;
            properties[5] = matchFinder;
            properties[6] = endMarker;

            if (!EncoderCache.TryGetValue(options.DictionarySize, out var cache)) {
                throw new NotSupportedException("Dictionary size not support: " + options.DictionarySize);
            }
            if (!cache.TryDequeue(out var lzmaEncoder)) {
                lzmaEncoder = new Encoder();
            }
            lzmaEncoder.SetCoderProperties(PropIDs, properties);
            lzmaEncoder.WriteCoderProperties(output);
            var fileSize = input.Length;
            for (var i = 0; i < 8; i++) {
                output.WriteByte((byte) (fileSize >> (8 * i)));
            }

            var progress = ObjectPool<ActionCodeProgress>.Shared.Get();
            progress.Create(onProgress);

            lzmaEncoder.Code(input, output, -1, -1, progress);

            ObjectPool<ActionCodeProgress>.Shared.Return(progress);

            cache.Enqueue(lzmaEncoder);
            ObjectArrayCache.Enqueue(properties);
        }

        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            if (!ByteArrayCache.TryDequeue(out var properties)) {
                properties = new byte[5];
            }
            if (input.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");

            uint dictionarySize = 0;
            for (var i = 0; i < 4; i++) {
                dictionarySize += (uint) properties[1 + i] << (i * 8);
            }

            if (!DecoderCache.TryGetValue((LZMACompressOptions.LZMADictionarySize) dictionarySize, out var cache)) {
                throw new NotSupportedException("Dictionary size not support: " + dictionarySize);
            }

            if (!cache.TryDequeue(out var decoder)) {
                decoder = new Decoder();
            }

            decoder.SetDecoderProperties(properties);
            long fileLength = 0;
            for (var i = 0; i < 8; i++) {
                var v = input.ReadByte();
                if (v < 0) throw new Exception("Can't Read 1");
                fileLength |= (long) (byte) v << (8 * i);
            }

            var progress = ObjectPool<ActionCodeProgress>.Shared.Get();
            progress.Create(onProgress);

            var compressedSize = input.Length - input.Position;
            decoder.Code(input, output, compressedSize, fileLength, progress);

            ObjectPool<ActionCodeProgress>.Shared.Return(progress);

            cache.Enqueue(decoder);
            ByteArrayCache.Enqueue(properties);
        }

        private class ActionCodeProgress : BaseObject<Action<long, long>>, ICodeProgress
        {
            private static Action<long, long> DefaultHandler => (inSize, outSize) => {};
            private Action<long, long> _handler;

            protected override void OnCreate(Action<long, long> handler) {
                _handler = handler ?? DefaultHandler;
            }
            protected override void OnDestroy() {
                _handler = null;
            }
            public void SetProgress(long inSize, long outSize) => _handler(inSize, outSize);
        }
    }
}