// ------------------------------------------------------------
//         File: SingletonException.cs
//        Brief: Exception types for singleton creation failures.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 17:57:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public class SingletonException : vFrameException
    {
        public SingletonException() { }

        public SingletonException(string message) : base(message) { }
    }

    [Obsolete("This exception is no longer thrown.")]
    public class SingletonDuplicatedException : SingletonException
    {
        public SingletonDuplicatedException() { }

        public SingletonDuplicatedException(string message) : base(message) { }
    }
}