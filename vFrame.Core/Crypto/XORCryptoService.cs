//------------------------------------------------------------
//        File:  XORCryptoService.cs
//       Brief:  XORCryptoService
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-13 20:43
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

namespace vFrame.Core.Crypto
{
    public sealed class XorCryptoService : CryptoService
    {
        public override void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            XORBuffer(input, output, key, keyLength);
        }

        public override void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            XORBuffer(input, output, key, keyLength);
        }

        private void XORBuffer(byte[] input, byte[] output, byte[] key, int keyLength) {
            for (var i = 0; i < input.Length; i++) {
                output[i] = (byte) (input[i] ^ key[i % keyLength]);
            }
        }
    }
}