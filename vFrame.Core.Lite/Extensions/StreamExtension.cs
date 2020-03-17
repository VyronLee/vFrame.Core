using System.Buffers;
using System.IO;

namespace vFrame.Core.Extensions
{
    public static class StreamExtension
    {
        public static void BufferedCopyTo(this Stream fromStream, Stream toStream, int size) {
            var byteArrayPool = ArrayPool<byte>.Shared;
            var buffer = byteArrayPool.Rent(size);

            var count = fromStream.Read(buffer, 0, size);
            if (count != size) {
                throw new InvalidDataException($"size expected: {size}, got: {count}");
            }
            toStream.Write(buffer, 0, size);
            byteArrayPool.Return(buffer);
        }
    }
}