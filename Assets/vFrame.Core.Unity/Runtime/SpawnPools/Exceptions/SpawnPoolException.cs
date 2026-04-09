// ------------------------------------------------------------
//         File: SpawnPoolException.cs
//        Brief: Base exception type for spawn pool errors.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using vFrame.Core;

namespace vFrame.Core.Unity
{
    public class SpawnPoolException : vFrameException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnPoolException"/> class.
        /// </summary>
        public SpawnPoolException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnPoolException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SpawnPoolException(string message) : base(message) { }
    }
}
