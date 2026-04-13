// ------------------------------------------------------------
//         File: IStreamEncryptor.cs
//        Brief: Extended encryptor interface with convenience
//                methods for auto-allocating buffer operations
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2025-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    /// Extended encryptor interface with convenience methods for auto-allocating buffer operations.
    /// </summary>
    public interface IStreamEncryptor : IEncryptor
    {
        /// <summary>
        /// Encrypts the given data and returns a new byte array with the result.
        /// The caller must determine the required output size based on the encryptor type.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        /// <returns>A new byte array containing the encrypted data.</returns>
        byte[] Encrypt(byte[] data, byte[] key, int keyLength) {
            ThrowHelper.ThrowIfNull(data, nameof(data));
            var output = new byte[data.Length];
            Encrypt(data, output, key, keyLength);
            return output;
        }

        /// <summary>
        /// Decrypts the given data and returns a new byte array with the result.
        /// The caller must determine the required output size based on the encryptor type.
        /// </summary>
        /// <param name="data">The data to decrypt.</param>
        /// <param name="key">The decryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        /// <returns>A new byte array containing the decrypted data.</returns>
        byte[] Decrypt(byte[] data, byte[] key, int keyLength) {
            ThrowHelper.ThrowIfNull(data, nameof(data));
            var output = new byte[data.Length];
            Decrypt(data, output, key, keyLength);
            return output;
        }
    }
}
