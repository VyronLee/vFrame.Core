using System;

namespace vFrame.Core.Coroutine
{
    public class CoroutinePoolException : Exception
    {
        public CoroutinePoolException() {

        }

        public CoroutinePoolException(string message) : base(message) {

        }
    }

    public class CoroutinePoolInvalidStateException : CoroutinePoolException
    {
        public CoroutinePoolInvalidStateException(string message) : base(message) {

        }
    }
}