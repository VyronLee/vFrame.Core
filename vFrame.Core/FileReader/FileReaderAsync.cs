//------------------------------------------------------------
//        File:  FileReaderAsync.cs
//       Brief:  FileReaderAsync
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-14 10:30
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using vFrame.Core.Bundler.Interface;
using vFrame.Core.Interface.Crypto;

namespace vFrame.Core.Bundler.FileReader
{
    public class FileReaderAsync : FileReader, IFileReaderAsync
    {
        public FileReaderAsync(ICryptoService crypto, byte[] key, int keyLength)
            :base(crypto, key, keyLength)
        {
        }

        public IFileReaderRequest ReadAllBytesAsync(string path)
        {
            return new FileReaderRequest(path, _crypto, _key, _keyLength);
        }
    }
}