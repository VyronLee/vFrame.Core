// ------------------------------------------------------------
//         File: CompressPool.cs
//        Brief: CompressPool.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-18 22:55
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.IO;
using vFrame.Core.Base;
using vFrame.Core.Exceptions;
using vFrame.Core.Generic;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Encryption
{
    public class EncryptorPool : Singleton<EncryptorPool>
    {
        private ObjectPoolManager _poolManager;

        protected override void OnCreate() {
            _poolManager = new ObjectPoolManager();
            _poolManager.Create();
        }

        protected override void OnDestroy() {
            _poolManager?.Destroy();
            _poolManager = null;
        }

        public IEncryptor Rent(EncryptorType encryptorType) {
            Encryptor encryptor = null;
            switch (encryptorType) {
                case EncryptorType.Plain:
                    encryptor = _poolManager.GetObjectPool<PlainEncryptor>().Get();
                    break;
                case EncryptorType.Xor:
                    encryptor = _poolManager.GetObjectPool<XOREncryptor>().Get();
                    break;
                default:
                    ThrowHelper.ThrowUnsupportedEnum(encryptorType);
                    break;
            }
            if (null == encryptor) {
                return null;
            }
            encryptor.Create();

            var wrap = _poolManager.GetObjectPool<EncryptorWrap>().Get();
            wrap.Create(this, encryptor);
            return wrap;
        }

        public void Return(IEncryptor encryptor) {
            _poolManager.Return(encryptor);
        }
    }

    public class EncryptorWrap : BaseObject<EncryptorPool, Encryptor>, IEncryptor
    {
        private Encryptor _encryptor;
        private EncryptorPool _pool;

        public void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            _encryptor.Encrypt(input, output, key, keyLength);
        }

        public void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            _encryptor.Decrypt(input, output, key, keyLength);
        }

        public void Encrypt(Stream input, Stream output, byte[] key, int keyLength) {
            _encryptor.Encrypt(input, output, key, keyLength);
        }

        public void Decrypt(Stream input, Stream output, byte[] key, int keyLength) {
            _encryptor.Decrypt(input, output, key, keyLength);
        }

        protected override void OnCreate(EncryptorPool pool, Encryptor compressor) {
            _pool = pool;
            _encryptor = compressor;
        }

        protected override void OnDestroy() {
            _pool.Return(_encryptor);
            _pool.Return(this);
            _pool = null;
            _encryptor = null;
        }
    }
}