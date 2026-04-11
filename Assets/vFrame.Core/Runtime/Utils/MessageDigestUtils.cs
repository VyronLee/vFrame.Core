// ------------------------------------------------------------
//         File: MessageDigestUtils.cs
//        Brief: Utility class providing MD5, SHA256, and SHA512 hash computation.
//                Uses factory Create() pattern instead of deprecated *CryptoServiceProvider.
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
        private static readonly byte[] EmptyBuffer = new byte[0];

        /// <summary>
        /// Computes the MD5 hash of a byte array as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 32-character lowercase hexadecimal MD5 string.</returns>
        public static string MD5(byte[] data) {
            return ComputeHashAsString("MD5", data);
        }

        /// <summary>
        /// Computes the MD5 hash of a stream as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="data">The input stream.</param>
        /// <returns>A 32-character lowercase hexadecimal MD5 string.</returns>
        public static string MD5(Stream data) {
            return ComputeHashAsString("MD5", data);
        }

        /// <summary>
        /// Computes the MD5 hash of a byte array as a raw byte array.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 16-byte MD5 hash.</returns>
        public static byte[] MD5b(byte[] data) {
            return ComputeHash("MD5", data);
        }

        /// <summary>
        /// Computes the MD5 hash of a stream as a raw byte array.
        /// </summary>
        /// <param name="data">The input stream.</param>
        /// <returns>A 16-byte MD5 hash.</returns>
        public static byte[] MD5b(Stream data) {
            return ComputeHash("MD5", data);
        }

        /// <summary>
        /// Computes the SHA256 hash of a byte array as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 64-character lowercase hexadecimal SHA256 string.</returns>
        public static string SHA256(byte[] data) {
            return ComputeHashAsString("SHA256", data);
        }

        /// <summary>
        /// Computes the SHA256 hash of a byte array as a raw byte array.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 32-byte SHA256 hash.</returns>
        public static byte[] SHA256b(byte[] data) {
            return ComputeHash("SHA256", data);
        }

        /// <summary>
        /// Computes the SHA512 hash of a byte array as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 128-character lowercase hexadecimal SHA512 string.</returns>
        public static string SHA512(byte[] data) {
            return ComputeHashAsString("SHA512", data);
        }

        /// <summary>
        /// Computes the SHA512 hash of a byte array as a raw byte array.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>A 64-byte SHA512 hash.</returns>
        public static byte[] SHA512b(byte[] data) {
            return ComputeHash("SHA512", data);
        }

        /// <summary>
        /// Computes the MD5 hash of a file as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A 32-character lowercase hexadecimal MD5 string.</returns>
        public static string FileMD5(string filePath) {
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                return ComputeHashAsString("MD5", file);
            }
        }

        /// <summary>
        /// Computes the MD5 hash of a file as a raw byte array.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A 16-byte MD5 hash.</returns>
        public static byte[] FileMD5b(string filePath) {
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                return ComputeHash("MD5", file);
            }
        }

        private static byte[] ComputeHash(string algorithmName, byte[] data) {
            using (var hash = HashAlgorithm.Create(algorithmName)) {
                return hash.ComputeHash(data);
            }
        }

        private static byte[] ComputeHash(string algorithmName, Stream data) {
            using (var hash = HashAlgorithm.Create(algorithmName)) {
                return hash.ComputeHash(data);
            }
        }

        private static string ComputeHashAsString(string algorithmName, byte[] data) {
            using (var hash = HashAlgorithm.Create(algorithmName)) {
                var ret = hash.ComputeHash(data);
                return ret.ToHex("x2");
            }
        }

        private static string ComputeHashAsString(string algorithmName, Stream data) {
            using (var hash = HashAlgorithm.Create(algorithmName)) {
                var ret = hash.ComputeHash(data);
                return ret.ToHex("x2");
            }
        }
    }
}
