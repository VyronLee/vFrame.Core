// ------------------------------------------------------------
//         File: XOREncryptor.cs
//        Brief: XOR encryptor that encrypts and decrypts data using a key
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-05-13 20:43:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using System.IO;

namespace vFrame.Core
{
    public sealed class XOREncryptor : Encryptor
    {
        /// <inheritdoc/>
        public override void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            XORBuffer(input, output, key, keyLength);
        }

        /// <inheritdoc/>
        public override void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            XORBuffer(input, output, key, keyLength);
        }

        /// <inheritdoc/>
        public override void Encrypt(Stream input, Stream output, byte[] key, int keyLength) {
            XORStream(input, output, key, keyLength);
        }

        /// <inheritdoc/>
        public override void Decrypt(Stream input, Stream output, byte[] key, int keyLength) {
            XORStream(input, output, key, keyLength);
        }

        /// <summary>
        /// Performs XOR operation on a byte array using the given key.
        /// </summary>
        /// <param name="input">The input data to transform.</param>
        /// <param name="output">The output buffer to receive transformed data.</param>
        /// <param name="key">The XOR key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        private static void XORBuffer(IReadOnlyList<byte> input, IList<byte> output, IReadOnlyList<byte> key,
            int keyLength) {
            for (var i = 0; i < input.Count; i++) {
                output[i] = (byte)(input[i] ^ key[i % keyLength]);
            }
        }

        /// <summary>
        /// Performs XOR operation on a stream using the given key.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="output">The output stream to write to.</param>
        /// <param name="key">The XOR key.</param>
        /// <param name="keyLength">The effective length of the key.</param>
        private static void XORStream(Stream input, Stream output, IReadOnlyList<byte> key, int keyLength) {
            for (var i = 0; i < input.Length; i++) {
                output.WriteByte((byte)(input.ReadByte() ^ key[i % keyLength]));
            }
        }
    }
}
