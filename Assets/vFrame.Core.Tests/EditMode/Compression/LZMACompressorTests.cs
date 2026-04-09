using System.IO;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Compression
{
    public class LZMACompressorTests
    {
        [Test]
        public void CompressAndDecompress_ReusesProgressHelperWithoutLeakingPreviousCallback() {
            var compressor = new LZMACompressor();
            compressor.Create(new LZMACompressorOptions {
                DictionarySize = LZMACompressorOptions.LZMADictionarySize.Small,
                Speed = LZMACompressorOptions.LZMASpeed.Fast
            });

            using var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            using var compressed = new MemoryStream();
            using var output = new MemoryStream();

            var firstProgressCount = 0;
            compressor.Compress(input, compressed, (inSize, outSize) => firstProgressCount++);
            var firstProgressCountAfterCompress = firstProgressCount;

            compressed.Position = 0;

            var secondProgressCount = 0;
            compressor.Decompress(compressed, output, (inSize, outSize) => secondProgressCount++);

            var bytes = output.ToArray();

            Assert.That(bytes, Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            Assert.That(firstProgressCount, Is.EqualTo(firstProgressCountAfterCompress));
        }
    }
}
