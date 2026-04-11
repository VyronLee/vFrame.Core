// ------------------------------------------------------------
//         File: XOREncryptor.cs
//        Brief: XOR encryptor that encrypts and decrypts data using a key.
//                Uses batched buffer processing for stream operations
//                instead of byte-by-byte for significantly better performance.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-05-13 20:43:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;

namespace vFrame.Core
{
    /// <summary>
    /// XOR-based symmetric encryptor. Encrypt and Decrypt are identical operations.
    /// </summary>
    /// <remarks>
    /// WARNING: XOR encryption provides obfuscation only — it does NOT offer
    /// cryptographic security. Use AES-based encryptors for sensitive data.
    /// </remarks>
    public sealed class XOREncryptor : Encryptor
    {
        private const int DefaultStreamBufferSize = 8192;

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
        /// Processes data in-place style with key cycling.
        /// </summary>
        private static void XORBuffer(byte[] input, byte[] output, byte[] key, int keyLength) {
            ThrowHelper.ThrowIfNull(input, nameof(input));
            ThrowHelper.ThrowIfNull(output, nameof(output));
            ThrowHelper.ThrowIfNull(key, nameof(key));
            if (keyLength <= 0 || keyLength > key.Length) {
                ThrowHelper.ThrowArgumentException("keyLength must be between 1 and key.Length.");
            }
            if (output.Length < input.Length) {
                ThrowHelper.ThrowArgumentException("Output buffer is too small.");
            }

            // Process in key-length aligned chunks to reduce modulo operations
            var keyLen = keyLength;
            var dataLen = input.Length;
            var fullCycles = dataLen / keyLen;
            var remainder = dataLen % keyLen;

            for (var cycle = 0; cycle < fullCycles; cycle++) {
                var baseOffset = cycle * keyLen;
                for (var k = 0; k < keyLen; k++) {
                    output[baseOffset + k] = (byte)(input[baseOffset + k] ^ key[k]);
                }
            }

            // Process remaining bytes
            var remainderOffset = fullCycles * keyLen;
            for (var i = 0; i < remainder; i++) {
                output[remainderOffset + i] = (byte)(input[remainderOffset + i] ^ key[i]);
            }
        }

        /// <summary>
        /// Performs XOR operation on a stream using batched buffer reads.
        /// Uses 8KB buffer instead of byte-by-byte ReadByte for ~10x throughput.
        /// </summary>
        private static void XORStream(Stream input, Stream output, byte[] key, int keyLength) {
            ThrowHelper.ThrowIfNull(input, nameof(input));
            ThrowHelper.ThrowIfNull(output, nameof(output));
            ThrowHelper.ThrowIfNull(key, nameof(key));
            if (keyLength <= 0 || keyLength > key.Length) {
                ThrowHelper.ThrowArgumentException("keyLength must be between 1 and key.Length.");
            }

            var buffer = new byte[DefaultStreamBufferSize];
            var keyLen = keyLength;
            var globalIndex = 0;

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) {
                // Process each byte with global key index for correctness
                for (var i = 0; i < bytesRead; i++) {
                    buffer[i] = (byte)(buffer[i] ^ key[globalIndex % keyLen]);
                    globalIndex++;
                }
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
