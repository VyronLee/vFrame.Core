// ------------------------------------------------------------
//         File: CryptoUtils.cs
//        Brief: Cryptographic utility methods for key derivation
//                and random byte generation
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2025-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Security.Cryptography;

namespace vFrame.Core
{
    /// <summary>
    ///     Provides common cryptographic utility methods.
    /// </summary>
    public static class CryptoUtils
    {
        /// <summary>
        ///     Derives a cryptographic key from a password using PBKDF2.
        /// </summary>
        /// <param name="password">The password bytes to derive the key from.</param>
        /// <param name="salt">The salt bytes. Must be at least 8 bytes.</param>
        /// <param name="iterations">The number of PBKDF2 iterations. Must be positive.</param>
        /// <param name="keyLength">The desired key length in bytes. Must be positive.</param>
        /// <returns>The derived key bytes.</returns>
        public static byte[] DeriveKey(byte[] password, byte[] salt, int iterations, int keyLength) {
            ThrowHelper.ThrowIfNull(password, nameof(password));
            ThrowHelper.ThrowIfNull(salt, nameof(salt));
            if (iterations <= 0) {
                ThrowHelper.ThrowArgumentException("Iterations must be positive.");
            }

            if (keyLength <= 0) {
                ThrowHelper.ThrowArgumentException("Key length must be positive.");
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256)) {
                return deriveBytes.GetBytes(keyLength);
            }
        }

        /// <summary>
        ///     Generates cryptographically secure random bytes.
        /// </summary>
        /// <param name="length">The number of bytes to generate. Must be non-negative.</param>
        /// <returns>A byte array filled with random bytes.</returns>
        public static byte[] GenerateRandomBytes(int length) {
            if (length < 0) {
                ThrowHelper.ThrowArgumentException("Length must be non-negative.");
            }

            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }
    }
}