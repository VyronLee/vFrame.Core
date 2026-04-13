// ------------------------------------------------------------
//         File: StreamExtension.cs
//        Brief: Stream extension method for buffered copy
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-09 16:56
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.IO;
using ByteArrayPool = System.Buffers.ArrayPool<byte>;

namespace vFrame.Core
{
    public static class StreamExtension
    {
        /// <summary>
        ///     Copies a fixed number of bytes from the source stream to the destination stream using a pooled buffer.
        /// </summary>
        /// <param name="fromStream">The source stream.</param>
        /// <param name="toStream">The destination stream.</param>
        /// <param name="size">The number of bytes to copy.</param>
        /// <exception cref="System.IO.InvalidDataException">Thrown when the source stream does not contain enough data.</exception>
        public static void BufferedCopyTo(this Stream fromStream, Stream toStream, int size) {
            var byteArrayPool = ByteArrayPool.Shared;
            var buffer = byteArrayPool.Rent(size);

            try {
                var count = fromStream.Read(buffer, 0, size);
                if (count != size) {
                    ThrowHelper.ThrowInvalidDataException($"size expected: {size}, got: {count}");
                }

                toStream.Write(buffer, 0, size);
            }
            finally {
                byteArrayPool.Return(buffer);
            }
        }
    }
}