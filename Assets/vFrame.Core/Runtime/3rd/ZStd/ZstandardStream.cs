using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Interop = vFrame.Core.ThirdParty.ZStd.ZstandardInterop;

namespace vFrame.Core.ThirdParty.ZStd
{
    /// <summary>
    ///     Provides methods and properties for compressing and decompressing streams by using the Zstandard algorithm.
    /// </summary>
    public class ZstandardStream : Stream
    {
        private readonly ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
        private readonly ZstandardInterop.Buffer inputBuffer = new ZstandardInterop.Buffer();
        private readonly bool leaveOpen;
        private readonly CompressionMode mode;

        private readonly ZstandardInterop.Buffer outputBuffer = new ZstandardInterop.Buffer();
        private readonly Stream stream;

        private readonly IntPtr zstream;
        private readonly uint zstreamInputSize;
        private readonly uint zstreamOutputSize;

        private byte[] data;
        private bool dataDepleted;
        private int dataPosition;
        private int dataSize;
        private bool dataSkipRead;
        private bool isClosed;
        private bool isDisposed;
        private bool isInitialized;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZstandardStream" /> class by using the specified stream and
        ///     compression mode, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The stream to compress.</param>
        /// <param name="mode">One of the enumeration values that indicates whether to compress or decompress the stream.</param>
        /// <param name="leaveOpen">
        ///     true to leave the stream open after disposing the <see cref="ZstandardStream" /> object;
        ///     otherwise, false.
        /// </param>
        public ZstandardStream(Stream stream, CompressionMode mode, bool leaveOpen = false) {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.mode = mode;
            this.leaveOpen = leaveOpen;

            if (mode == CompressionMode.Compress) {
                zstreamInputSize = Interop.ZSTD_CStreamInSize().ToUInt32();
                zstreamOutputSize = Interop.ZSTD_CStreamOutSize().ToUInt32();
                zstream = Interop.ZSTD_createCStream();
                data = arrayPool.Rent((int)zstreamOutputSize);
            }

            if (mode == CompressionMode.Decompress) {
                zstreamInputSize = Interop.ZSTD_DStreamInSize().ToUInt32();
                zstreamOutputSize = Interop.ZSTD_DStreamOutSize().ToUInt32();
                zstream = Interop.ZSTD_createDStream();
                data = arrayPool.Rent((int)zstreamInputSize);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZstandardStream" /> class  by using the specified stream and
        ///     compression level, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The stream to compress.</param>
        /// <param name="compressionLevel">The compression level.</param>
        /// <param name="leaveOpen">
        ///     true to leave the stream open after disposing the <see cref="ZstandardStream" /> object;
        ///     otherwise, false.
        /// </param>
        public ZstandardStream(Stream stream, int compressionLevel, bool leaveOpen = false) : this(stream,
            CompressionMode.Compress, leaveOpen) {
            CompressionLevel = compressionLevel;
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------

        /// <summary>
        ///     The version of the native Zstd library.
        /// </summary>
        public static Version Version {
            get {
                var version = (int)Interop.ZSTD_versionNumber();
                return new Version(version / 10000 % 100, version / 100 % 100, version % 100);
            }
        }

        /// <summary>
        ///     The maximum compression level supported by the native Zstd library.
        /// </summary>
        public static int MaxCompressionLevel => Interop.ZSTD_maxCLevel();

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------

        /// <summary>
        ///     Gets or sets the compression level to use, the default is 6.
        /// </summary>
        /// <remarks>
        ///     To get the maximum compression level see <see cref="MaxCompressionLevel" />.
        /// </remarks>
        public int CompressionLevel { get; set; } = 6;

        /// <summary>
        ///     Gets or sets the compression dictionary tp use, the default is null.
        /// </summary>
        /// <value>
        ///     The compression dictionary.
        /// </value>
        public ZstandardDictionary CompressionDictionary { get; set; } = null;

        /// <summary>
        ///     Gets whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => stream.CanRead && mode == CompressionMode.Decompress;

        /// <summary>
        ///     Gets whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => stream.CanWrite && mode == CompressionMode.Compress;

        /// <summary>
        ///     Gets whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        public override long Length => throw new NotSupportedException();

        /// <summary>
        ///     Gets or sets the position within the current stream.
        /// </summary>
        public override long Position {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (!isDisposed) {
                if (!isClosed) {
                    ReleaseResources(false);
                }

                arrayPool.Return(data, false);
                isDisposed = true;
                data = null;
            }
        }

        public override void Close() {
            if (isClosed) {
                return;
            }

            try {
                ReleaseResources(true);
            }
            finally {
                isClosed = true;
                base.Close();
            }
        }

        private void ReleaseResources(bool flushStream) {
            if (mode == CompressionMode.Compress) {
                try {
                    if (flushStream) {
                        ProcessStream((zcs, buffer) => Interop.ThrowIfError(Interop.ZSTD_flushStream(zcs, buffer)));
                        ProcessStream((zcs, buffer) => Interop.ThrowIfError(Interop.ZSTD_endStream(zcs, buffer)));
                        stream.Flush();
                    }
                }
                finally {
                    Interop.ZSTD_freeCStream(zstream);
                    if (!leaveOpen) {
                        stream.Close();
                    }
                }
            }
            else if (mode == CompressionMode.Decompress) {
                Interop.ZSTD_freeDStream(zstream);
                if (!leaveOpen) {
                    stream.Close();
                }
            }
        }

        public override void Flush() {
            if (mode == CompressionMode.Compress) {
                ProcessStream((zcs, buffer) => Interop.ThrowIfError(Interop.ZSTD_flushStream(zcs, buffer)));
                stream.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (!CanRead) {
                throw new NotSupportedException();
            }

            // prevent the buffers from being moved around by the garbage collector
            var alloc1 = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var alloc2 = GCHandle.Alloc(data, GCHandleType.Pinned);

            try {
                var length = 0;

                if (!isInitialized) {
                    isInitialized = true;

                    var result = CompressionDictionary == null
                        ? Interop.ZSTD_initDStream(zstream)
                        : Interop.ZSTD_initDStream_usingDDict(zstream,
                            CompressionDictionary.GetDecompressionDictionary());
                }

                while (count > 0) {
                    var inputSize = dataSize - dataPosition;

                    // read data from input stream
                    if (inputSize <= 0 && !dataDepleted && !dataSkipRead) {
                        dataSize = stream.Read(data, 0, (int)zstreamInputSize);
                        dataDepleted = dataSize <= 0;
                        dataPosition = 0;
                        inputSize = dataDepleted ? 0 : dataSize;

                        // skip stream.Read until the internal buffer is depleted
                        // avoids a Read timeout for applications that know the exact number of bytes in the stream
                        dataSkipRead = true;
                    }

                    // configure the inputBuffer
                    inputBuffer.Data = inputSize <= 0
                        ? IntPtr.Zero
                        : Marshal.UnsafeAddrOfPinnedArrayElement(data, dataPosition);
                    inputBuffer.Size = inputSize <= 0 ? UIntPtr.Zero : new UIntPtr((uint)inputSize);
                    inputBuffer.Position = UIntPtr.Zero;

                    // configure the outputBuffer
                    outputBuffer.Data = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
                    outputBuffer.Size = new UIntPtr((uint)count);
                    outputBuffer.Position = UIntPtr.Zero;

                    // decompress inputBuffer to outputBuffer
                    Interop.ThrowIfError(Interop.ZSTD_decompressStream(zstream, outputBuffer, inputBuffer));

                    // calculate progress in outputBuffer
                    var outputBufferPosition = (int)outputBuffer.Position.ToUInt32();
                    if (outputBufferPosition == 0) {
                        // the internal buffer is depleted, we're either done
                        if (dataDepleted) {
                            break;
                        }

                        // or we need more bytes
                        dataSkipRead = false;
                    }

                    length += outputBufferPosition;
                    offset += outputBufferPosition;
                    count -= outputBufferPosition;

                    // calculate progress in inputBuffer
                    var inputBufferPosition = (int)inputBuffer.Position.ToUInt32();
                    dataPosition += inputBufferPosition;
                }

                return length;
            }
            finally {
                alloc1.Free();
                alloc2.Free();
            }
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (!CanWrite) {
                throw new NotSupportedException();
            }

            // prevent the buffers from being moved around by the garbage collector
            var alloc1 = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var alloc2 = GCHandle.Alloc(data, GCHandleType.Pinned);

            try {
                if (!isInitialized) {
                    isInitialized = true;

                    var result = CompressionDictionary == null
                        ? Interop.ZSTD_initCStream(zstream, CompressionLevel)
                        : Interop.ZSTD_initCStream_usingCDict(zstream,
                            CompressionDictionary.GetCompressionDictionary(CompressionLevel));

                    Interop.ThrowIfError(result);
                }

                while (count > 0) {
                    var inputSize = Math.Min((uint)count, zstreamInputSize);

                    // configure the outputBuffer
                    outputBuffer.Data = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
                    outputBuffer.Size = new UIntPtr(zstreamOutputSize);
                    outputBuffer.Position = UIntPtr.Zero;

                    // configure the inputBuffer
                    inputBuffer.Data = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
                    inputBuffer.Size = new UIntPtr(inputSize);
                    inputBuffer.Position = UIntPtr.Zero;

                    // compress inputBuffer to outputBuffer
                    Interop.ThrowIfError(Interop.ZSTD_compressStream(zstream, outputBuffer, inputBuffer));

                    // write data to output stream
                    var outputBufferPosition = (int)outputBuffer.Position.ToUInt32();
                    stream.Write(data, 0, outputBufferPosition);

                    // calculate progress in inputBuffer
                    var inputBufferPosition = (int)inputBuffer.Position.ToUInt32();
                    offset += inputBufferPosition;
                    count -= inputBufferPosition;
                }
            }
            finally {
                alloc1.Free();
                alloc2.Free();
            }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------

        private void ProcessStream(Action<IntPtr, ZstandardInterop.Buffer> outputAction) {
            var alloc = GCHandle.Alloc(data, GCHandleType.Pinned);

            try {
                outputBuffer.Data = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
                outputBuffer.Size = new UIntPtr(zstreamOutputSize);
                outputBuffer.Position = UIntPtr.Zero;

                outputAction(zstream, outputBuffer);

                var outputBufferPosition = (int)outputBuffer.Position.ToUInt32();
                stream.Write(data, 0, outputBufferPosition);
            }
            finally {
                alloc.Free();
            }
        }
    }
}