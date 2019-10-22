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
using vFrame.Core.Crypto;
using vFrame.Core.Loggers;
using vFrame.Core.Utils;

namespace vFrame.Core.FileReader
{
    public class FileReader : IFileReader
    {
        protected ICryptoService _crypto;
        protected byte[] _key;
        protected int _keyLength;

        private static bool _betterStreamingAssetsInited;

        public FileReader()
        {
            if (_betterStreamingAssetsInited)
                return;
            _betterStreamingAssetsInited = true;

            BetterStreamingAssets.Initialize();
        }

        public FileReader(ICryptoService crypto, byte[] key, int keyLength)
        {
            _crypto = crypto;
            _key = key;
            _keyLength = keyLength;
        }

        public bool FileExist(string path)
        {
#if UNITY_ANDROID
            if (!PathUtils.IsFileInPersistentDataPath(path))
            {
                var relativePath = PathUtils.AbsolutePathToRelativeStreamingAssetsPath(path);
                return BetterStreamingAssets.FileExists(relativePath);
            }
            else
#endif
            {
                return File.Exists(path);
            }
        }

        public virtual byte[] ReadAllBytes(string path)
        {
            byte[] bytes = null;
#if UNITY_ANDROID
            if (!PathUtils.IsFileInPersistentDataPath(path))
            {
                var relativePath = PathUtils.AbsolutePathToRelativeStreamingAssetsPath(path);
                bytes = BetterStreamingAssets.ReadAllBytes(relativePath);
            }
            else
#endif
            {
                bytes = File.ReadAllBytes(path);
            }
            return DecryptIfRequired(bytes, path);
        }

        public virtual string ReadAllText(string path)
        {
            var text = string.Empty;
#if UNITY_ANDROID
            if (!PathUtils.IsFileInPersistentDataPath(path))
            {
                var relativePath = PathUtils.AbsolutePathToRelativeStreamingAssetsPath(path);
                //Logger.Error("relativePath: {0}", relativePath);
                text = BetterStreamingAssets.ReadAllText(relativePath);
            }
            else
#endif
            {
                text = File.ReadAllText(path);
            }
            return text;
        }

        public bool IsFileExist(string path)
        {
#if UNITY_ANDROID
            if (!PathUtils.IsFileInPersistentDataPath(path))
            {
                var relativePath = PathUtils.AbsolutePathToRelativeStreamingAssetsPath(path);
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

            Logger.Info("Decrypt data started: {0}", path);

            var output = new byte[bytes.Length];
            _crypto.Decrypt(bytes, output, _key, _keyLength);

            Logger.Info("Decrypt data finished: {0}", path);

            return output;
        }

    }
}