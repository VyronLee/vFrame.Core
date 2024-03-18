// ------------------------------------------------------------
//         File: VFrameException.cs
//        Brief: VFrameException.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-17 22:35
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

// ReSharper disable InconsistentNaming

using System;

namespace vFrame.Core.Exceptions
{
    public class vFrameException : Exception
    {
        public vFrameException() {}
        public vFrameException(string message) : base(message) {}
    }

    public class vFrameArgumentException : vFrameException
    {
        public vFrameArgumentException(string message) : base(message) {}
    }

    public class vFrameArgumentNullException : vFrameException
    {
        public vFrameArgumentNullException(string name) : base(name) {}
    }

    public class vFrameUnsupportedEnumException : vFrameException
    {
        public vFrameUnsupportedEnumException(string message) : base(message) {}
    }
}