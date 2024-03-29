//------------------------------------------------------------
//        File:  CryptoService.cs
//       Brief:  CryptoService
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-05-13 20:39
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.IO;
using vFrame.Core.Base;

namespace vFrame.Core.Encryption
{
    public abstract class Encryptor : BaseObject, IEncryptor
    {
        public abstract void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);
        public abstract void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);

        public abstract void Encrypt(Stream input, Stream output, byte[] key, int keyLength);
        public abstract void Decrypt(Stream input, Stream output, byte[] key, int keyLength);

        protected override void OnCreate() { }

        protected override void OnDestroy() { }
    }
}