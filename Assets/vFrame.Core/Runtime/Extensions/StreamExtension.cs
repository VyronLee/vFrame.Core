using System.IO;
using vFrame.Core.Exceptions;
using ByteArrayPool = System.Buffers.ArrayPool<byte>;

namespace vFrame.Core.Extensions
{
    public static class StreamExtension
    {
        public static void BufferedCopyTo(this Stream fromStream, Stream toStream, int size) {
            var byteArrayPool = ByteArrayPool.Shared;
            var buffer = byteArrayPool.Rent(size);

            var count = fromStream.Read(buffer, 0, size);
            if (count != size) {
                ThrowHelper.ThrowInvalidDataException($"size expected: {size}, got: {count}");
            }
            toStream.Write(buffer, 0, size);
            byteArrayPool.Return(buffer);
        }
    }
}