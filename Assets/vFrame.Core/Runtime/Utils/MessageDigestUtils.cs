// ------------------------------------------------------------
//         File: MessageDigestUtils.cs
//        Brief: Utility class providing MD5, SHA256, and SHA512 hash computation
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-09 17:20
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.IO;
using System.Security.Cryptography;

namespace vFrame.Core
{
    public static class MessageDigestUtils
    {
        /// <summary>
        /// Computes the MD5 hash of a byte array as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 32-character lowercase hexadecimal MD5 string.</returns>
        public static string MD5(byte[] data) {
            var md5 = new MD5CryptoServiceProvider();
            var ret = md5.ComputeHash(data);
            return ret.ToHex("x2");
        }

        /// <summary>
        /// Computes the MD5 hash of a stream as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="data">The input stream.</param>
        /// <returns>A 32-character lowercase hexadecimal MD5 string.</returns>
        public static string MD5(Stream data) {
            var md5 = new MD5CryptoServiceProvider();
            var ret = md5.ComputeHash(data);
            return ret.ToHex("x2");
        }

        /// <summary>
        /// Computes the MD5 hash of a byte array as a raw byte array.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 16-byte MD5 hash.</returns>
        public static byte[] MD5b(byte[] data) {
            var md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(data);
        }

        /// <summary>
        /// Computes the MD5 hash of a stream as a raw byte array.
        /// </summary>
        /// <param name="data">The input stream.</param>
        /// <returns>A 16-byte MD5 hash.</returns>
        public static byte[] MD5b(Stream data) {
            var md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(data);
        }

        /// <summary>
        /// Computes the SHA256 hash of a byte array as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 64-character lowercase hexadecimal SHA256 string.</returns>
        public static string SHA256(byte[] data) {
            var sha = new SHA256CryptoServiceProvider();
            var ret = sha.ComputeHash(data);
            return ret.ToHex("x2");
        }

        /// <summary>
        /// Computes the SHA256 hash of a byte array as a raw byte array.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 32-byte SHA256 hash.</returns>
        public static byte[] SHA256b(byte[] data) {
            var sha = new SHA256CryptoServiceProvider();
            return sha.ComputeHash(data);
        }

        /// <summary>
        /// Computes the SHA512 hash of a byte array as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 128-character lowercase hexadecimal SHA512 string.</returns>
        public static string SHA512(byte[] data) {
            var sha = new SHA512CryptoServiceProvider();
            var ret = sha.ComputeHash(data);
            return ret.ToHex("x2");
        }

        /// <summary>
        /// Computes the SHA512 hash of a byte array as a raw byte array.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 64-byte SHA512 hash.</returns>
        public static byte[] SHA512b(byte[] data) {
            var sha = new SHA512CryptoServiceProvider();
            return sha.ComputeHash(data);
        }

        /// <summary>
        /// Computes the MD5 hash of a file as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A 32-character lowercase hexadecimal MD5 string.</returns>
        public static string FileMD5(string filePath) {
            byte[] retVal;
            using (var file = new FileStream(filePath, FileMode.Open)) {
                var md5 = new MD5CryptoServiceProvider();
                retVal = md5.ComputeHash(file);
            }

            return retVal.ToHex("x2");
        }

        /// <summary>
        /// Computes the MD5 hash of a file as a raw byte array.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A 16-byte MD5 hash.</returns>
        public static byte[] FileMD5b(string filePath) {
            using (var file = new FileStream(filePath, FileMode.Open)) {
                var md5 = new MD5CryptoServiceProvider();
                return md5.ComputeHash(file);
            }
        }
    }
}
