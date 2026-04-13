// ------------------------------------------------------------
//         File: HmacSha256.cs
//        Brief: HMAC-SHA256 utility for data integrity verification
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
    ///     Provides HMAC-SHA256 hash computation and verification.
    /// </summary>
    public static class HmacSha256
    {
        /// <summary>
        ///     Computes the HMAC-SHA256 hash of the given data using the specified key.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <param name="key">The HMAC key.</param>
        /// <returns>A 32-byte HMAC-SHA256 hash.</returns>
        public static byte[] ComputeHash(byte[] data, byte[] key) {
            ThrowHelper.ThrowIfNull(data, nameof(data));
            ThrowHelper.ThrowIfNull(key, nameof(key));

            using (var hmac = new HMACSHA256(key)) {
                return hmac.ComputeHash(data);
            }
        }

        /// <summary>
        ///     Verifies that the HMAC-SHA256 hash of the given data matches the expected hash.
        /// </summary>
        /// <param name="data">The data to verify.</param>
        /// <param name="key">The HMAC key.</param>
        /// <param name="expectedHash">The expected 32-byte hash.</param>
        /// <returns>True if the computed hash matches the expected hash; otherwise, false.</returns>
        public static bool VerifyHash(byte[] data, byte[] key, byte[] expectedHash) {
            ThrowHelper.ThrowIfNull(data, nameof(data));
            ThrowHelper.ThrowIfNull(key, nameof(key));
            ThrowHelper.ThrowIfNull(expectedHash, nameof(expectedHash));
            if (expectedHash.Length != 32) {
                ThrowHelper.ThrowArgumentException(
                    "Expected hash must be 32 bytes for HMAC-SHA256.");
            }

            var computedHash = ComputeHash(data, key);
            return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
        }
    }
}