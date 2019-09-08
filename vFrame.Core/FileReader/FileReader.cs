//------------------------------------------------------------
//        File:  FileReader.cs
//       Brief:  FileReader
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-14 10:29
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.IO;
using vFrame.Core.Bundler.Interface;
using vFrame.Core.Interface.Crypto;

namespace vFrame.Core.Bundler.FileReader
{
    public class FileReader : IFileReader
    {
        protected ICryptoService _crypto;
        protected byte[] _key;
        protected int _keyLength;

        public FileReader()
        {

        }

        public FileReader(ICryptoService crypto, byte[] key, int keyLength)
        {
            _crypto = crypto;
            _key = key;
            _keyLength = keyLength;
        }

        public virtual byte[] ReadAllBytes(string path)
        {
            byte[] bytes = null;
#if UNITY_ANDROID
            if (!PathUtility.IsFileInPersistentDataPath(path))
            {
                var relativePath = PathUtility.AbsolutePathToRelativeStreamingAssetsPath(path);
                bytes = BetterStreamingAssets.ReadAllBytes(relativePath);
            }
            else
#endif
            {
                bytes = File.ReadAllBytes(path);
            }
            return DecryptIfRequired(bytes, path);
        }

        public bool IsFileExist(string path)
        {
#if UNITY_ANDROID
            if (!PathUtility.IsFileInPersistentDataPath(path))
            {
                var relativePath = PathUtility.AbsolutePathToRelativeStreamingAssetsPath(path);
                return BetterStreamingAssets.FileExists(relativePath);
            }
            else
#endif
            {
                return File.Exists(path);
            }
        }

        private byte[] DecryptIfRequired(byte[] bytes, string path)
        {
            if (null == _crypto)
                return bytes;

            Logger.LogInfo("Decrypt data started: {0}", path);

            var output = new byte[bytes.Length];
            _crypto.Decrypt(bytes, output, _key, _keyLength);

            Logger.LogInfo("Decrypt data finished: {0}", path);

            return output;
        }

    }
}