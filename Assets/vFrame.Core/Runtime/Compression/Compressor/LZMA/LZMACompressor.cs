// ------------------------------------------------------------
//         File: LZMACompressor.cs
//        Brief: LZMA algorithm-based compressor implementation
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using vFrame.Core.ThirdParty.SevenZip;
using vFrame.Core.ThirdParty.SevenZip.Compression.LZMA;

namespace vFrame.Core
{
    public class LZMACompressor : Compressor
    {
        private static readonly LZMACompressorOptions DefaultOptions = new LZMACompressorOptions {
            DictionarySize = LZMACompressorOptions.LZMADictionarySize.Small,
            Speed = LZMACompressorOptions.LZMASpeed.Medium
        };

        private static readonly Dictionary<LZMACompressorOptions.LZMADictionarySize, ConcurrentQueue<Encoder>>
            EncoderCache
                = new Dictionary<LZMACompressorOptions.LZMADictionarySize, ConcurrentQueue<Encoder>> {
                    { LZMACompressorOptions.LZMADictionarySize.VerySmall, new ConcurrentQueue<Encoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.Small, new ConcurrentQueue<Encoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.Medium, new ConcurrentQueue<Encoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.Large, new ConcurrentQueue<Encoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.Larger, new ConcurrentQueue<Encoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.VeryLarge, new ConcurrentQueue<Encoder>() }
                };

        private static readonly Dictionary<LZMACompressorOptions.LZMADictionarySize, ConcurrentQueue<Decoder>>
            DecoderCache
                = new Dictionary<LZMACompressorOptions.LZMADictionarySize, ConcurrentQueue<Decoder>> {
                    { LZMACompressorOptions.LZMADictionarySize.VerySmall, new ConcurrentQueue<Decoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.Small, new ConcurrentQueue<Decoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.Medium, new ConcurrentQueue<Decoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.Large, new ConcurrentQueue<Decoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.Larger, new ConcurrentQueue<Decoder>() },
                    { LZMACompressorOptions.LZMADictionarySize.VeryLarge, new ConcurrentQueue<Decoder>() }
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

        /// <summary>
        ///     Called when the compressor is created. Falls back to default options if none are provided.
        /// </summary>
        /// <param name="options">The compressor options, or null to use defaults.</param>
        protected override void OnCreate(CompressorOptions options) {
            base.OnCreate(options ?? DefaultOptions);
        }

        /// <summary>
        ///     Compresses the input stream using the LZMA algorithm.
        /// </summary>
        /// <param name="input">The stream containing data to compress.</param>
        /// <param name="output">The stream to write compressed data to.</param>
        /// <param name="onProgress">Progress callback invoked with in-size and out-size.</param>
        /// <exception cref="NotSupportedException">Thrown when the configured dictionary size is not supported.</exception>
        public override void Compress(Stream input, Stream output, Action<long, long> onProgress) {
            var options = (LZMACompressorOptions)Options;

            const int posStateBits = 2; // default: 2
            const int litContextBits = 3; // 3 for normal files, 0; for 32-bit data
            const int litPosBits = 0; // 0 for 64-bit data, 2 for 32-bit.
            const string matchFinder = "BT4"; // default: BT4
            const bool endMarker = true;

            if (!ObjectArrayCache.TryDequeue(out var properties)) {
                properties = new object[7];
            }

            properties[0] = (int)options.DictionarySize;
            properties[1] = posStateBits;
            properties[2] = litContextBits;
            properties[3] = litPosBits;
            properties[4] = (int)options.Speed;
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
                output.WriteByte((byte)(fileSize >> (8 * i)));
            }

            var progress = ObjectPool<ActionCodeProgress>.Shared.Get();
            progress.Initialize(onProgress);

            lzmaEncoder.Code(input, output, -1, -1, progress);

            ObjectPool<ActionCodeProgress>.Shared.Return(progress);

            cache.Enqueue(lzmaEncoder);
            ObjectArrayCache.Enqueue(properties);
        }

        /// <summary>
        ///     Decompresses the LZMA-compressed input stream.
        /// </summary>
        /// <param name="input">The stream containing LZMA-compressed data.</param>
        /// <param name="output">The stream to write decompressed data to.</param>
        /// <param name="onProgress">Progress callback invoked with in-size and out-size.</param>
        /// <exception cref="NotSupportedException">Thrown when the dictionary size in the compressed header is not supported.</exception>
        /// <exception cref="Exception">Thrown when the input stream is too short or cannot be read.</exception>
        public override void Decompress(Stream input, Stream output, Action<long, long> onProgress) {
            if (!ByteArrayCache.TryDequeue(out var properties)) {
                properties = new byte[5];
            }

            if (input.Read(properties, 0, 5) != 5) {
                throw new Exception("input .lzma is too short");
            }

            uint dictionarySize = 0;
            for (var i = 0; i < 4; i++) {
                dictionarySize += (uint)properties[1 + i] << (i * 8);
            }

            if (!DecoderCache.TryGetValue((LZMACompressorOptions.LZMADictionarySize)dictionarySize, out var cache)) {
                throw new NotSupportedException("Dictionary size not support: " + dictionarySize);
            }

            if (!cache.TryDequeue(out var decoder)) {
                decoder = new Decoder();
            }

            decoder.SetDecoderProperties(properties);
            long fileLength = 0;
            for (var i = 0; i < 8; i++) {
                var v = input.ReadByte();
                if (v < 0) {
                    throw new Exception("Can't Read 1");
                }

                fileLength |= (long)(byte)v << (8 * i);
            }

            var progress = ObjectPool<ActionCodeProgress>.Shared.Get();
            progress.Initialize(onProgress);

            var compressedSize = input.Length - input.Position;
            decoder.Code(input, output, compressedSize, fileLength, progress);

            ObjectPool<ActionCodeProgress>.Shared.Return(progress);

            cache.Enqueue(decoder);
            ByteArrayCache.Enqueue(properties);
        }

        private class ActionCodeProgress : ICodeProgress, IPoolObjectResetable
        {
            private static readonly Action<long, long> DefaultHandler = (inSize, outSize) => { };
            private Action<long, long> _handler = DefaultHandler;

            /// <summary>
            ///     Reports compression progress by invoking the registered handler.
            /// </summary>
            /// <param name="inSize">Number of bytes processed from the input.</param>
            /// <param name="outSize">Number of bytes written to the output.</param>
            public void SetProgress(long inSize, long outSize) {
                _handler(inSize, outSize);
            }

            /// <summary>
            ///     Resets the progress handler to the default no-op.
            /// </summary>
            public void Reset() {
                _handler = DefaultHandler;
            }

            /// <summary>
            ///     Initializes the progress handler with the given callback.
            /// </summary>
            /// <param name="handler">The progress callback, or null to use a no-op handler.</param>
            public void Initialize(Action<long, long> handler) {
                _handler = handler ?? DefaultHandler;
            }
        }
    }
}