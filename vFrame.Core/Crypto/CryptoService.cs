//------------------------------------------------------------
//        File:  CryptoService.cs
//       Brief:  CryptoService
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-13 20:39
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using vFrame.Core.Interface.Crypto;

namespace vFrame.Core.Crypto
{
    public abstract class CryptoService : ICryptoService
    {
        public abstract void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);
        public abstract void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);

        public static ICryptoService CreateCrypto(CryptoType type)
        {
            switch (type)
            {
                case CryptoType.PLAIN:
                    return new PlainCryptoService();
                case CryptoType.XOR:
                    return new XORCryptoService();
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
        }
    }
}