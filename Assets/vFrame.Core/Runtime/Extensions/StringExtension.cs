// ------------------------------------------------------------
//         File: StringExtension.cs
//        Brief: String extension methods for byte array conversion and hex parsing
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-09 16:59
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace vFrame.Core
{
    public static class StringExtension
    {
        /// <summary>
        ///     Converts the string to a byte sequence using the default encoding.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <returns>A byte sequence.</returns>
        public static IEnumerable<byte> ToBytes(this string str) {
            var byteArray = Encoding.Default.GetBytes(str);
            return byteArray;
        }

        /// <summary>
        ///     Converts the string to a byte array using the default encoding.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToByteArray(this string str) {
            var byteArray = Encoding.Default.GetBytes(str);
            return byteArray;
        }

        /// <summary>
        ///     Converts the string to a byte array using UTF-8 encoding.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <returns>A UTF-8 encoded byte array.</returns>
        public static byte[] ToUtf8ByteArray(this string str) {
            var byteArray = Encoding.UTF8.GetBytes(str);
            return byteArray;
        }

        /// <summary>
        ///     Converts a hexadecimal string to a byte array.
        /// </summary>
        /// <param name="hexString">The hexadecimal string, whose length must be even.</param>
        /// <returns>A byte array.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the hexadecimal string has an odd length.</exception>
        public static byte[] HexToBytes(this string hexString) {
            ThrowHelper.ThrowIfNull(hexString, nameof(hexString));

            if (hexString.Length == 0) {
                return Array.Empty<byte>();
            }

            if (hexString.Length % 2 != 0) {
                ThrowHelper.ThrowArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            var hexAsBytes = new byte[hexString.Length / 2];
            for (var index = 0; index < hexAsBytes.Length; index++) {
                var high = ParseHexChar(hexString[index * 2]);
                var low = ParseHexChar(hexString[index * 2 + 1]);
                hexAsBytes[index] = (byte)((high << 4) | low);
            }

            return hexAsBytes;
        }

        /// <summary>
        ///     Parses a single hex character to its numeric value.
        /// </summary>
        private static int ParseHexChar(char c) {
            if (c >= '0' && c <= '9') {
                return c - '0';
            }

            if (c >= 'a' && c <= 'f') {
                return c - 'a' + 10;
            }

            if (c >= 'A' && c <= 'F') {
                return c - 'A' + 10;
            }

            ThrowHelper.ThrowArgumentException($"Invalid hex character: {c}");
            return 0;
        }
    }
}