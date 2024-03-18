using vFrame.Core.Exceptions;

namespace vFrame.Core.Compress
{
    public class BlockBasedCompressionException : vFrameException
    {
        public BlockBasedCompressionException(){}
        public BlockBasedCompressionException(string message) : base(message) {}
    }

    public class InvalidBlockBasedCompressionFormatException : BlockBasedCompressionException
    {

    }

    public class BlockTableDataErrorException : BlockBasedCompressionException
    {

    }

    public class DataNotEnoughException : BlockBasedCompressionException
    {

    }

    public class IndexOutOfRangeException : BlockBasedCompressionException
    {

    }

    public class HashNotMatchException : BlockBasedCompressionException
    {

    }

    public class StateBusyException : BlockBasedCompressionException
    {

    }

    public class BufferSizeTooSmallException : BlockBasedCompressionException
    {
        public BufferSizeTooSmallException(byte[] buffer, long excepted)
            : base($"Buffer too small: {buffer.Length}, excepted: {excepted}") {
        }
    }

    public class BufferSizeTooLargeException : BlockBasedCompressionException
    {
        public BufferSizeTooLargeException(long outLength) : base($"Size too large: {outLength}") {

        }
    }
}