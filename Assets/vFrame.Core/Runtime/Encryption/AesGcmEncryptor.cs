// ------------------------------------------------------------
//         File: AesGcmEncryptor.cs
//        Brief: AES-GCM authenticated encryptor using AES-256-GCM
//                Output format: [12-byte nonce][ciphertext][16-byte tag]
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2025-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;
using System.Security.Cryptography;

namespace vFrame.Core
{
    /// <summary>
    /// AES-GCM authenticated symmetric encryptor using AES-256.
    /// </summary>
    /// <remarks>
    /// Output format: [12-byte nonce][ciphertext][16-byte tag]
    /// Requires a 32-byte key (AES-256). Uses System.Security.Cryptography.AesGcm
    /// (.NET Standard 2.1 / Unity 2022.3+).
    /// </remarks>
    public sealed class AesGcmEncryptor : Encryptor
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int RequiredKeyLength = 32;

        /// <inheritdoc/>
        public override void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            ThrowHelper.ThrowIfNull(input, nameof(input));
            ThrowHelper.ThrowIfNull(output, nameof(output));
            ThrowHelper.ThrowIfNull(key, nameof(key));
            if (keyLength != RequiredKeyLength) {
                ThrowHelper.ThrowArgumentException(
                    $"Key length must be {RequiredKeyLength} for AES-256-GCM, got {keyLength}.");
            }
            if (output.Length < input.Length + NonceSize + TagSize) {
                ThrowHelper.ThrowArgumentException("Output buffer is too small.");
            }

            var nonce = new byte[NonceSize];
            var tag = new byte[TagSize];
            RandomNumberGenerator.Fill(nonce);

            using (var aesGcm = new AesGcm(key)) {
                aesGcm.Encrypt(nonce, input, output.AsSpan(NonceSize), output.AsSpan(NonceSize + input.Length, TagSize));
            }

            nonce.CopyTo(output.AsSpan(0, NonceSize));
            tag.CopyTo(output.AsSpan(NonceSize + input.Length, TagSize));
        }

        /// <inheritdoc/>
        public override void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            ThrowHelper.ThrowIfNull(input, nameof(input));
            ThrowHelper.ThrowIfNull(output, nameof(output));
            ThrowHelper.ThrowIfNull(key, nameof(key));
            if (keyLength != RequiredKeyLength) {
                ThrowHelper.ThrowArgumentException(
                    $"Key length must be {RequiredKeyLength} for AES-256-GCM, got {keyLength}.");
            }
            var minInputLength = NonceSize + TagSize;
            if (input.Length < minInputLength) {
                ThrowHelper.ThrowArgumentException(
                    $"Input is too short to contain nonce and tag (minimum {minInputLength} bytes).");
            }
            var cipherLength = input.Length - NonceSize - TagSize;
            if (output.Length < cipherLength) {
                ThrowHelper.ThrowArgumentException("Output buffer is too small.");
            }

            var nonce = input.AsSpan(0, NonceSize).ToArray();
            var cipherText = input.AsSpan(NonceSize, cipherLength);
            var tag = input.AsSpan(NonceSize + cipherLength, TagSize).ToArray();

            using (var aesGcm = new AesGcm(key)) {
                aesGcm.Decrypt(nonce, cipherText, tag, output);
            }
        }

        /// <inheritdoc/>
        public override void Encrypt(Stream input, Stream output, byte[] key, int keyLength) {
            ThrowHelper.ThrowIfNull(input, nameof(input));
            ThrowHelper.ThrowIfNull(output, nameof(output));

            using (var ms = new MemoryStream()) {
                input.CopyTo(ms);
                var inputData = ms.ToArray();

                var outputData = new byte[inputData.Length + NonceSize + TagSize];
                Encrypt(inputData, outputData, key, keyLength);
                output.Write(outputData, 0, outputData.Length);
            }
        }

        /// <inheritdoc/>
        public override void Decrypt(Stream input, Stream output, byte[] key, int keyLength) {
            ThrowHelper.ThrowIfNull(input, nameof(input));
            ThrowHelper.ThrowIfNull(output, nameof(output));

            using (var ms = new MemoryStream()) {
                input.CopyTo(ms);
                var inputData = ms.ToArray();

                var outputData = new byte[inputData.Length - NonceSize - TagSize];
                Decrypt(inputData, outputData, key, keyLength);
                output.Write(outputData, 0, outputData.Length);
            }
        }
    }
}
