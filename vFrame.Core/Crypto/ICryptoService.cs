//------------------------------------------------------------
//        File:  ICryptoService.cs
//       Brief:  Crypto service interface
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-13 20:35
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

namespace vFrame.Core.Crypto
{
    public interface ICryptoService
    {
        void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);
        void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);
    }
}