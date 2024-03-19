//------------------------------------------------------------
//        File:  ICryptoService.cs
//       Brief:  Crypto service interface
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-05-13 20:35
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.IO;

namespace vFrame.Core.Encryption
{
    public interface IEncryptor : IDisposable
    {
        void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);
        void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);

        void Encrypt(Stream input, Stream output, byte[] key, int keyLength);
        void Decrypt(Stream input, Stream output, byte[] key, int keyLength);
    }
}