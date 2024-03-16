using System;

namespace vFrame.Core.Compress.BlockBasedCompression
{
    public class BlockBasedCompressionException : Exception
    {

    }

    public class InvalidBlockBasedCompressionFormatException : Exception
    {

    }

    public class BlockBasedCompressionBlockTableDataErrorException : Exception
    {

    }

    public class BlockBasedCompressionDataNotEnoughException : Exception
    {

    }

    public class BlockBasedCompressionIndexOutOfRangeException : Exception
    {

    }

    public class BlockBasedCompressionHashNotMatchException : Exception
    {

    }

    public class BlockBasedCompressionStateBusyException : Exception
    {

    }

    public class BlockBasedCompressionBufferTooSmallException : Exception
    {
        public BlockBasedCompressionBufferTooSmallException(byte[] buffer, long excepted)
            : base($"Buffer too small: {buffer.Length}, excepted: {excepted}") {
        }
    }

    public class BlockBasedCompressionBufferSizeTooLargeException : Exception
    {
        public BlockBasedCompressionBufferSizeTooLargeException() : base() {

        }

        public BlockBasedCompressionBufferSizeTooLargeException(long outLength) : base($"Size too large: {outLength}") {

        }
    }
}