// ------------------------------------------------------------
//         File: SingletonException.cs
//        Brief: SingletonException.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-18 17:57
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using vFrame.Core.Exceptions;

namespace vFrame.Core.Singletons
{
    public class SingletonException : vFrameException
    {

    }

    public class SingletonDuplicatedException : SingletonException
    {

    }
}