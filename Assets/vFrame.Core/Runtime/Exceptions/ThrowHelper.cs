// ------------------------------------------------------------
//         File: ThrowHelper.cs
//        Brief: Static helper methods for throwing common exception types
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2023-12-25 22:58:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace vFrame.Core
{
    /// <summary>
    /// Provides static helper methods for throwing common vFrame exception types.
    /// </summary>
    public static class ThrowHelper
    {
        /// <summary>
        /// Throws an <see cref="ArgumentException"/> with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public static void ThrowArgumentException(string message) {
            throw new ArgumentException(message);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the parameter is null.
        /// </summary>
        /// <param name="param">The value to check for null.</param>
        /// <param name="variable">The name of the parameter.</param>
        public static void ThrowIfNull(object param, string variable) {
            if (null != param) {
                return;
            }
            throw new ArgumentNullException(variable);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the collection is null or empty.
        /// </summary>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        /// <param name="param">The collection to check.</param>
        /// <param name="variable">The name of the parameter.</param>
        public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> param, string variable) {
            if (null != param && param.Any()) {
                return;
            }
            throw new ArgumentException($"Variable ${variable} cannot be null or empty!");
        }

        /// <summary>
        /// Throws an <see cref="UnsupportedEnumException"/> for the given enum value.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The unsupported enum value.</param>
        public static void ThrowUnsupportedEnum<T>(T value) {
            throw new UnsupportedEnumException($"Unsupported enum value: {value}!");
        }

        /// <summary>
        /// Throws an <see cref="InvalidDataException"/> with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public static void ThrowInvalidDataException(string message) {
            throw new InvalidDataException(message);
        }

        /// <summary>
        /// Throws a generic <see cref="vFrameException"/> with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public static void ThrowUndesiredException(string message) {
            throw new vFrameException(message);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the string is null or empty.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void ThrowIfEmpty(string value, string paramName) {
            if (!string.IsNullOrEmpty(value)) {
                return;
            }
            throw new ArgumentException($"Parameter '{paramName}' cannot be null or empty!");
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the value is outside the specified range.
        /// </summary>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The minimum valid value (inclusive).</param>
        /// <param name="max">The maximum valid value (inclusive).</param>
        public static void ThrowIfOutOfRange(string paramName, int value, int min, int max) {
            if (value >= min && value <= max) {
                return;
            }
            throw new ArgumentException(
                $"Parameter '{paramName}' value {value} is out of range [{min}, {max}].");
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the enum value is not defined in the enum type.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void ThrowIfEnumDefined<T>(T value, string paramName) {
            if (Enum.IsDefined(typeof(T), value)) {
                return;
            }
            throw new ArgumentException(
                $"Parameter '{paramName}' value '{value}' is not defined in enum '{typeof(T).Name}'.");
        }

        /// <summary>
        /// Throws a <see cref="TypeMismatchException"/> if the input type does not match the desired type.
        /// </summary>
        /// <param name="inputType">The actual type to check.</param>
        /// <param name="desiredType">The expected type.</param>
        public static void ThrowIfTypeMismatch(Type inputType, Type desiredType) {
            if (inputType == desiredType) {
                return;
            }
            throw new TypeMismatchException(inputType, desiredType);
        }

        /// <summary>
        /// Throws an <see cref="IndexOutOfRangeException"/> for the given range and input value.
        /// </summary>
        /// <param name="start">The start of the valid range.</param>
        /// <param name="end">The end of the valid range.</param>
        /// <param name="input">The actual index value that is out of range.</param>
        public static void ThrowIndexOutOfRangeException(int start, int end, int input) {
            throw new IndexOutOfRangeException(start, end, input);
        }

        /// <summary>
        /// Concatenates variable name segments into a dot-separated path string.
        /// </summary>
        /// <param name="args">Variable name segments to join.</param>
        /// <returns>A dot-separated string of the variable names, or an empty string if no args are provided.</returns>
        public static string Variables(params string[] args) {
            if (null == args || args.Length <= 0) {
                return "";
            }
            return string.Join(".", args);
        }
    }
}
