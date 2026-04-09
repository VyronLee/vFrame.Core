// ------------------------------------------------------------
//         File: EnumUtils.cs
//        Brief: Utility class providing enum index lookup and string parsing
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-09 17:01
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public static class EnumUtils
    {
        /// <summary>
        /// Gets the index position of an enum value within its enum definition.
        /// </summary>
        /// <param name="value">The integer value of the enum.</param>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>The zero-based index, or -1 if not found.</returns>
        public static int EnumIndex<T>(int value) {
            return EnumIndex(typeof(T), value);
        }

        /// <summary>
        /// Gets the index position of an enum value within the specified enum type.
        /// </summary>
        /// <param name="type">The enum type.</param>
        /// <param name="value">The integer value of the enum.</param>
        /// <returns>The zero-based index, or -1 if not found.</returns>
        public static int EnumIndex(Type type, int value) {
            var i = 0;
            foreach (var v in Enum.GetValues(type)) {
                if ((int)v == value) {
                    return i;
                }
                ++i;
            }

            return -1;
        }

        /// <summary>
        /// Parses an enum value from its string name.
        /// </summary>
        /// <param name="str">The enum name string.</param>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>The parsed enum value.</returns>
        /// <exception cref="System.Exception">Thrown when the string does not match any defined enum value.</exception>
        public static T FromString<T>(string str) {
            return (T)FromString(typeof(T), str);
        }

        /// <summary>
        /// Parses an enum value from its string name for the specified enum type.
        /// </summary>
        /// <param name="type">The enum type.</param>
        /// <param name="str">The enum name string.</param>
        /// <returns>The parsed enum value as an object.</returns>
        /// <exception cref="System.Exception">Thrown when the string does not match any defined enum value.</exception>
        public static object FromString(Type type, string str) {
            if (!Enum.IsDefined(type, str)) {
                ThrowHelper.ThrowUndesiredException($"No enum value defined in type: {type.Name}, {str}");
            }
            return Enum.Parse(type, str);
        }
    }
}
