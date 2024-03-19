//------------------------------------------------------------
//        File:  XOREncryptor.cs
//       Brief:  XOREncryptor
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-05-13 20:43
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;
using System.IO;

namespace vFrame.Core.Encryption
{
    public sealed class XOREncryptor : Encryptor
    {
        public override void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            XORBuffer(input, output, key, keyLength);
        }

        public override void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            XORBuffer(input, output, key, keyLength);
        }

        public override void Encrypt(Stream input, Stream output, byte[] key, int keyLength) {
            XORStream(input, output, key, keyLength);
        }

        public override void Decrypt(Stream input, Stream output, byte[] key, int keyLength) {
            XORStream(input, output, key, keyLength);
        }

        private static void XORBuffer(IReadOnlyList<byte> input, IList<byte> output, IReadOnlyList<byte> key,
            int keyLength) {
            for (var i = 0; i < input.Count; i++) {
                output[i] = (byte)(input[i] ^ key[i % keyLength]);
            }
        }

        private static void XORStream(Stream input, Stream output, IReadOnlyList<byte> key, int keyLength) {
            for (var i = 0; i < input.Length; i++) {
                output.WriteByte((byte)(input.ReadByte() ^ key[i % keyLength]));
            }
        }
    }
}