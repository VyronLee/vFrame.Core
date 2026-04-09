// ------------------------------------------------------------
//         File: PlainEncryptor.cs
//        Brief: No-op encryptor that copies data without encryption
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-05-24 20:48:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;

namespace vFrame.Core
{
    public sealed class PlainEncryptor : Encryptor
    {
        /// <inheritdoc/>
        public override void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            Array.Copy(input, output, input.Length);
        }

        /// <inheritdoc/>
        public override void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            Array.Copy(input, output, input.Length);
        }

        /// <inheritdoc/>
        public override void Encrypt(Stream input, Stream output, byte[] key, int keyLength) {
            input.CopyTo(output);
        }

        /// <inheritdoc/>
        public override void Decrypt(Stream input, Stream output, byte[] key, int keyLength) {
            input.CopyTo(output);
        }
    }
}
