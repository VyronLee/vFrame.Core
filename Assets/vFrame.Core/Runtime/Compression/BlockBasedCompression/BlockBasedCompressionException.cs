// ------------------------------------------------------------
//         File: BlockBasedCompressionException.cs
//        Brief: Exception types for block-based compression operations
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================


namespace vFrame.Core
{
    public class BlockBasedCompressionException : vFrameException
    {
        /// <summary>
        /// Initializes a new block-based compression exception.
        /// </summary>
        public BlockBasedCompressionException() { }

        /// <summary>
        /// Initializes a new block-based compression exception with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        public BlockBasedCompressionException(string message) : base(message) { }
    }

    public class InvalidBlockBasedCompressionFormatException : BlockBasedCompressionException { }

    public class BlockTableDataErrorException : BlockBasedCompressionException { }

    public class DataNotEnoughException : BlockBasedCompressionException { }

    public class BlockIndexOutOfRangeException : BlockBasedCompressionException { }

    public class HashNotMatchException : BlockBasedCompressionException { }

    public class StateBusyException : BlockBasedCompressionException { }

    public class BufferSizeTooSmallException : BlockBasedCompressionException
    {
        /// <summary>
        /// Thrown when the provided buffer is too small for the required data size.
        /// </summary>
        /// <param name="buffer">The actual buffer that is too small.</param>
        /// <param name="excepted">The expected minimum buffer size.</param>
        public BufferSizeTooSmallException(byte[] buffer, long excepted)
            : base($"Buffer too small: {buffer.Length}, excepted: {excepted}") { }
    }

    public class BufferSizeTooLargeException : BlockBasedCompressionException
    {
        /// <summary>
        /// Thrown when the data size exceeds the maximum buffer capacity.
        /// </summary>
        /// <param name="outLength">The actual data size that exceeded the limit.</param>
        public BufferSizeTooLargeException(long outLength) : base($"Size too large: {outLength}") { }
    }
}