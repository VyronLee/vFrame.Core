// ------------------------------------------------------------
//         File: Encryptor.cs
//        Brief: Abstract base class providing a common implementation for encryptors
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-05-13 20:39:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.IO;

namespace vFrame.Core
{
    public abstract class Encryptor : BaseObject, IEncryptor
    {
        /// <summary>
        /// Encrypts the given byte array.
        /// </summary>
        /// <param name="input">The input data to encrypt.</param>
        /// <param name="output">The output buffer to receive encrypted data.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        public abstract void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);

        /// <summary>
        /// Decrypts the given byte array.
        /// </summary>
        /// <param name="input">The input data to decrypt.</param>
        /// <param name="output">The output buffer to receive decrypted data.</param>
        /// <param name="key">The decryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        public abstract void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);

        /// <summary>
        /// Encrypts the given stream.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="output">The output stream to write encrypted data to.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        public abstract void Encrypt(Stream input, Stream output, byte[] key, int keyLength);

        /// <summary>
        /// Decrypts the given stream.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="output">The output stream to write decrypted data to.</param>
        /// <param name="key">The decryption key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        public abstract void Decrypt(Stream input, Stream output, byte[] key, int keyLength);

        /// <summary>
        /// Called when the encryptor is created. No-op by default.
        /// </summary>
        protected override void OnCreate() { }

        /// <summary>
        /// Called when the encryptor is destroyed. No-op by default.
        /// </summary>
        protected override void OnDestroy() { }
    }
}
