using System;

namespace vFrame.Core.Unity.Coroutine
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

    public class CoroutineRunnerExistInIdleListException : CoroutinePoolException
    {
        public CoroutineRunnerExistInIdleListException(string message) : base(message) {

        }
    }
}