// ------------------------------------------------------------
//         File: SingletonException.cs
//        Brief: Exception types for singleton creation failures.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 17:57:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using vFrame.Core;

namespace vFrame.Core
{
    public class SingletonException : vFrameException { }

    public class SingletonDuplicatedException : SingletonException { }
}
