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
        public vFrameException() { }

        public vFrameException(string message) : base(message) { }
    }

    public class ArgumentException : vFrameException
    {
        public ArgumentException(string message) : base(message) { }
    }

    public class ArgumentNullException : vFrameException
    {
        public ArgumentNullException(string name) : base(name) { }
    }

    public class TypeMismatchException : vFrameException
    {
        public TypeMismatchException(Type inputType, Type desiredType)
            : base($"Type mismatch, desired: {desiredType.FullName}, got: {inputType.FullName}") { }
    }

    public class UnsupportedEnumException : vFrameException
    {
        public UnsupportedEnumException(string message) : base(message) { }
    }

    public class InvalidDataException : vFrameException
    {
        public InvalidDataException(string message) : base(message) { }
    }

    public class IndexOutOfRangeException : vFrameException
    {
        public IndexOutOfRangeException(int start, int end, int input)
            : base($"Range: [{start}, {end}], got: {input}") { }
    }

}