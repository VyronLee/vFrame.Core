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
using System.IO;
using vFrame.Core.Base;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Crypto
{
    public abstract class CryptoService : BaseObject, ICryptoService
    {
        public abstract void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);
        public abstract void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);

        public abstract void Encrypt(Stream input, Stream output, byte[] key, int keyLength);
        public abstract void Decrypt(Stream input, Stream output, byte[] key, int keyLength);

        protected override void OnCreate() {

        }

        protected override void OnDestroy() {

        }

        public static ICryptoService CreateCrypto(CryptoType type) {
            CryptoService service;
            switch (type) {
                case CryptoType.Plain:
                    service = ObjectPool<PlainCryptoService>.Get();
                    break;
                case CryptoType.Xor:
                    service = new XorCryptoService();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
            service.Create();
            return service;
        }
    }
}