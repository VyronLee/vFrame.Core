using System;
using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace vFrame.Core.Compress.LZMA
{
    public class LZMACompressService : CompressService
    {
        private static readonly LZMACompressOptions DefaultOptions = new LZMACompressOptions() {
            DictionarySize = LZMACompressOptions.LZMADictionarySize.Small,
            Speed = LZMACompressOptions.LZMASpeed.Medium,
        };

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

        public override void Compress(Stream input, Stream output) {
            var options = (LZMACompressOptions) Options;

            const int posStateBits = 2; // default: 2
            const int litContextBits = 3; // 3 for normal files, 0; for 32-bit data
            const int litPosBits = 0; // 0 for 64-bit data, 2 for 32-bit.
            const string matchFinder = "BT4"; // default: BT4
            const bool endMarker = true;

            object[] properties = {
                (int) options.DictionarySize,
                posStateBits,
                litContextBits,
                litPosBits,
                (int) options.Speed,
                matchFinder,
                endMarker
            };

            var lzmaEncoder = new Encoder();
            lzmaEncoder.SetCoderProperties(PropIDs, properties);
            lzmaEncoder.WriteCoderProperties(output);
            var fileSize = input.Length;
            for (var i = 0; i < 8; i++) output.WriteByte((byte) (fileSize >> (8 * i)));
            lzmaEncoder.Code(input, output, -1, -1, null);
        }

        public override void Decompress(Stream input, Stream output) {
            var decoder = new Decoder();

            var properties = new byte[5];
            if (input.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");

            decoder.SetDecoderProperties(properties);

            long fileLength = 0;
            for (var i = 0; i < 8; i++) {
                var v = input.ReadByte();
                if (v < 0) throw new Exception("Can't Read 1");
                fileLength |= (long) (byte) v << (8 * i);
            }

            var compressedSize = input.Length - input.Position;
            decoder.Code(input, output, compressedSize, fileLength, null);
        }
    }
}