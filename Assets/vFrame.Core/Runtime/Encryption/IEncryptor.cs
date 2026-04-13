// ------------------------------------------------------------
//         File: IEncryptor.cs
//        Brief: Interface defining unified encrypt and decrypt operations
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-05-13 20:35:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;

namespace vFrame.Core
{
    public interface IEncryptor : IDisposable
    {
        /// <summary>
        ///     Encrypts the given byte array.
        /// </summary>
        /// <param name="input">The input data to encrypt.</param>
        /// <param name="output">The output buffer to receive encrypted data.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);

        /// <summary>
        ///     Decrypts the given byte array.
        /// </summary>
        /// <param name="input">The input data to decrypt.</param>
        /// <param name="output">The output buffer to receive decrypted data.</param>
        /// <param name="key">The decryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);

        /// <summary>
        ///     Encrypts the given stream.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="output">The output stream to write encrypted data to.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        void Encrypt(Stream input, Stream output, byte[] key, int keyLength);

        /// <summary>
        ///     Decrypts the given stream.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="output">The output stream to write decrypted data to.</param>
        /// <param name="key">The decryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        void Decrypt(Stream input, Stream output, byte[] key, int keyLength);
    }
}