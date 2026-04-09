// ------------------------------------------------------------
//         File: CoroutinePoolException.cs
//        Brief: Exception hierarchy used by the coroutine pool to
//               report invalid state and configuration errors.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Base exception type for coroutine pool related errors.
    /// </summary>
    public class CoroutinePoolException : Exception
    {
        /// <summary>
        /// Initializes a new instance with no error message.
        /// </summary>
        public CoroutinePoolException() { }

        /// <summary>
        /// Initializes a new instance with the specified error message.
        /// </summary>
        /// <param name="message">Description of the error.</param>
        public CoroutinePoolException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a coroutine pool operation is performed in an invalid state.
    /// </summary>
    public class CoroutinePoolInvalidStateException : CoroutinePoolException
    {
        /// <summary>
        /// Initializes a new instance with the specified error message.
        /// </summary>
        /// <param name="message">Description of the state violation.</param>
        public CoroutinePoolInvalidStateException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a coroutine runner is unexpectedly found in the idle list.
    /// </summary>
    public class CoroutineRunnerExistInIdleListException : CoroutinePoolException
    {
        /// <summary>
        /// Initializes a new instance with the specified error message.
        /// </summary>
        /// <param name="message">Description of the conflict.</param>
        public CoroutineRunnerExistInIdleListException(string message) : base(message) { }
    }
}