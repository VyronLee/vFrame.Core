//------------------------------------------------------------
//        File:  PlainCryptoService.cs
//       Brief:  PlainCryptoService
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-24 20:48
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.IO;

namespace vFrame.Core.Crypto
{
    public sealed class PlainCryptoService : CryptoService
    {
        public override void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            Array.Copy(input, output, input.Length);
        }

        public override void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            Array.Copy(input, output, input.Length);
        }

        public override void Encrypt(Stream input, Stream output, byte[] key, int keyLength) {
            input.CopyTo(output);
        }

        public override void Decrypt(Stream input, Stream output, byte[] key, int keyLength) {
            input.CopyTo(output);
        }
    }
}