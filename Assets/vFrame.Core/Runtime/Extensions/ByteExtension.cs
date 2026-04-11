// ------------------------------------------------------------
//         File: ByteExtension.cs
//        Brief: Byte extension methods for hexadecimal conversion and string decoding
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-09 16:56
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Text;

namespace vFrame.Core
{
    public static class ByteExtension
    {
        /// <summary>
        /// Converts a single byte to an uppercase hexadecimal string.
        /// </summary>
        /// <param name="b">The byte value.</param>
        /// <returns>A two-character uppercase hexadecimal string.</returns>
        public static string ToHex(this byte b) {
            return b.ToString("X2");
        }

        /// <summary>
        /// Converts a byte array to an uppercase hexadecimal string.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>A concatenated uppercase hexadecimal string.</returns>
        public static string ToHex(this byte[] bytes) {
            ThrowHelper.ThrowIfNull(bytes, nameof(bytes));
            var builder = StringBuilderPool.Shared.Get();
            foreach (var b in bytes) {
                builder.Append(b.ToString("X2"));
            }
            var str = builder.ToString();
            StringBuilderPool.Shared.Return(builder);
            return str;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string using the specified format.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="format">The format string (e.g. "X2", "x2").</param>
        /// <returns>A concatenated hexadecimal string.</returns>
        public static string ToHex(this byte[] bytes, string format) {
            ThrowHelper.ThrowIfNull(bytes, nameof(bytes));
            var builder = StringBuilderPool.Shared.Get();
            foreach (var b in bytes) {
                builder.Append(b.ToString(format));
            }
            var str = builder.ToString();
            StringBuilderPool.Shared.Return(builder);
            return str;
        }

        /// <summary>
        /// Converts a specified range of a byte array to an uppercase hexadecimal string.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="offset">The starting offset.</param>
        /// <param name="count">The number of bytes to convert.</param>
        /// <returns>A concatenated uppercase hexadecimal string.</returns>
        public static string ToHex(this byte[] bytes, int offset, int count) {
            ThrowHelper.ThrowIfNull(bytes, nameof(bytes));
            if (offset < 0 || count < 0 || offset + count > bytes.Length) {
                ThrowHelper.ThrowArgumentException("Invalid offset or count for byte array.");
            }
            var builder = StringBuilderPool.Shared.Get();
            for (var i = offset; i < offset + count; ++i) {
                builder.Append(bytes[i].ToString("X2"));
            }
            var str = builder.ToString();
            StringBuilderPool.Shared.Return(builder);
            return str;
        }

        /// <summary>
        /// Converts a byte array to a string using the default encoding.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The decoded string.</returns>
        public static string ToStr(this byte[] bytes) {
            return Encoding.Default.GetString(bytes);
        }

        /// <summary>
        /// Converts a specified range of a byte array to a string using the default encoding.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="index">The starting index.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>The decoded string.</returns>
        public static string ToStr(this byte[] bytes, int index, int count) {
            return Encoding.Default.GetString(bytes, index, count);
        }

        /// <summary>
        /// Converts a byte array to a string using UTF-8 encoding.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The UTF-8 decoded string.</returns>
        public static string Utf8ToStr(this byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Converts a specified range of a byte array to a string using UTF-8 encoding.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="index">The starting index.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>The UTF-8 decoded string.</returns>
        public static string Utf8ToStr(this byte[] bytes, int index, int count) {
            return Encoding.UTF8.GetString(bytes, index, count);
        }
    }
}
