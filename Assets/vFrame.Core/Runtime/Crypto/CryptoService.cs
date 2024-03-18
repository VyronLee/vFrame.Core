//------------------------------------------------------------
//        File:  CryptoService.cs
//       Brief:  CryptoService
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-05-13 20:39
//   Copyright:  Copyright (c) 2024, VyronLee
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

        public static ICryptoService CreateCryptoService(CryptoType type) {
            CryptoService service;
            switch (type) {
                case CryptoType.Plain:
                    service = ObjectPool<PlainCryptoService>.Shared.Get();
                    break;
                case CryptoType.Xor:
                    service = ObjectPool<XorCryptoService>.Shared.Get();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            service.Create();
            return service;
        }

        public static void DestroyCryptoService(ICryptoService cryptoService) {
            cryptoService.Destroy();
            switch (cryptoService) {
                case PlainCryptoService plainCryptoService:
                    ObjectPool<PlainCryptoService>.Shared.Return(plainCryptoService);
                    break;
                case XorCryptoService xorCryptoService:
                    ObjectPool<XorCryptoService>.Shared.Return(xorCryptoService);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(cryptoService.GetType().FullName);
            }
        }
    }
}