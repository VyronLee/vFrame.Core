// ------------------------------------------------------------
//         File: vFrameException.cs
//        Brief: Core exception types for vFrame
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-17 22:35:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

// ReSharper disable InconsistentNaming

using System;

namespace vFrame.Core
{
    /// <summary>
    ///     Base exception for all vFrame errors.
    /// </summary>
    public class vFrameException : Exception
    {
        /// <summary>
        ///     Initializes a new vFrame exception.
        /// </summary>
        public vFrameException() { }

        /// <summary>
        ///     Initializes a new vFrame exception with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        public vFrameException(string message) : base(message) { }
    }

    /// <summary>
    ///     Exception thrown when an invalid argument is provided.
    /// </summary>
    public class ArgumentException : vFrameException
    {
        /// <summary>
        ///     Initializes a new argument exception with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        public ArgumentException(string message) : base(message) { }
    }

    /// <summary>
    ///     Exception thrown when an argument is null.
    /// </summary>
    public class ArgumentNullException : vFrameException
    {
        /// <summary>
        ///     Initializes a new argument null exception with the name of the null parameter.
        /// </summary>
        /// <param name="name">The name of the parameter that is null.</param>
        public ArgumentNullException(string name) : base(name) { }
    }

    /// <summary>
    ///     Exception thrown when a type does not match the expected type.
    /// </summary>
    public class TypeMismatchException : vFrameException
    {
        /// <summary>
        ///     Initializes a new type mismatch exception.
        /// </summary>
        /// <param name="inputType">The actual type that was provided.</param>
        /// <param name="desiredType">The expected type.</param>
        public TypeMismatchException(Type inputType, Type desiredType)
            : base($"Type mismatch, desired: {desiredType.FullName}, got: {inputType.FullName}") { }
    }

    /// <summary>
    ///     Exception thrown when an unsupported enum value is encountered.
    /// </summary>
    public class UnsupportedEnumException : vFrameException
    {
        /// <summary>
        ///     Initializes a new unsupported enum exception with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        public UnsupportedEnumException(string message) : base(message) { }
    }

    /// <summary>
    ///     Exception thrown when invalid data is encountered.
    /// </summary>
    public class InvalidDataException : vFrameException
    {
        /// <summary>
        ///     Initializes a new invalid data exception with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        public InvalidDataException(string message) : base(message) { }
    }

    /// <summary>
    ///     Exception thrown when an index is outside the valid range.
    /// </summary>
    public class IndexOutOfRangeException : vFrameException
    {
        /// <summary>
        ///     Initializes a new index out of range exception.
        /// </summary>
        /// <param name="start">The start of the valid range.</param>
        /// <param name="end">The end of the valid range.</param>
        /// <param name="input">The actual index value that is out of range.</param>
        public IndexOutOfRangeException(int start, int end, int input)
            : base($"Range: [{start}, {end}], got: {input}") { }
    }
}