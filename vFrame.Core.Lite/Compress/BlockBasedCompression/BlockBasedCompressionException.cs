using System;

namespace vFrame.Core.Compress.BlockBasedCompression
{
    public class BlockBasedCompressionException : Exception
    {

    }

    public class InvalidBlockBasedCompressionFormatException : Exception
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

    public class BlockBasedCompressionBufferSizeTooLargeException : Exception
    {
        public BlockBasedCompressionBufferSizeTooLargeException() : base() {

        }

        public BlockBasedCompressionBufferSizeTooLargeException(long outLength) : base($"Size too large: {outLength}") {

        }
    }
}