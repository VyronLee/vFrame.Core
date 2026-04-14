// ------------------------------------------------------------
//         File: EncryptorPool.cs
//        Brief: Object pool for encryptors, managing rental and return lifecycle
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.IO;

namespace vFrame.Core
{
    public class EncryptorPool : Singleton<EncryptorPool>
    {
        private ObjectPoolManager _poolManager;

        /// <summary>
        ///     Initializes the pool manager during singleton creation.
        /// </summary>
        protected override void OnCreate() {
            _poolManager = new ObjectPoolManager();
        }

        /// <summary>
        ///     Tears down the pool manager during singleton destruction.
        /// </summary>
        protected override void OnDestroy() {
            _poolManager?.Dispose();
            _poolManager = null;
        }

        /// <summary>
        ///     Rents an encryptor of the specified type from the object pool.
        /// </summary>
        /// <param name="encryptorType">The type of encryptor to rent.</param>
        /// <returns>An <see cref="IEncryptor" /> instance that must be returned after use.</returns>
        /// <exception cref="System.ArgumentException">Thrown when an unsupported encryptor type is specified.</exception>
        public IEncryptor Rent(EncryptorType encryptorType) {
            Encryptor encryptor = null;
            switch (encryptorType) {
                case EncryptorType.Plain:
                    encryptor = _poolManager.GetObjectPool<PlainEncryptor>().Get();
                    break;
                case EncryptorType.Xor:
                    encryptor = _poolManager.GetObjectPool<XOREncryptor>().Get();
                    break;
                case EncryptorType.AesGcm:
                    encryptor = _poolManager.GetObjectPool<AesGcmEncryptor>().Get();
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

        /// <summary>
        ///     Returns an encryptor to the object pool.
        /// </summary>
        /// <param name="encryptor">The encryptor to return.</param>
        public void Return(IEncryptor encryptor) {
            _poolManager.Return(encryptor);
        }
    }

    public class EncryptorWrap : BaseObject<EncryptorPool, Encryptor>, IEncryptor
    {
        private Encryptor _encryptor;
        private EncryptorPool _pool;

        /// <inheritdoc />
        public void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            _encryptor.Encrypt(input, output, key, keyLength);
        }

        /// <inheritdoc />
        public void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength) {
            _encryptor.Decrypt(input, output, key, keyLength);
        }

        /// <inheritdoc />
        public void Encrypt(Stream input, Stream output, byte[] key, int keyLength) {
            _encryptor.Encrypt(input, output, key, keyLength);
        }

        /// <inheritdoc />
        public void Decrypt(Stream input, Stream output, byte[] key, int keyLength) {
            _encryptor.Decrypt(input, output, key, keyLength);
        }

        /// <summary>
        ///     Initializes the wrap with the owning pool and the underlying encryptor.
        /// </summary>
        /// <param name="pool">The pool that owns this wrap.</param>
        /// <param name="encryptor">The underlying encryptor instance.</param>
        protected override void OnCreate(EncryptorPool pool, Encryptor encryptor) {
            _pool = pool;
            _encryptor = encryptor;
        }

        /// <summary>
        ///     Returns both the underlying encryptor and this wrap to the pool.
        /// </summary>
        protected override void OnDestroy() {
            _pool.Return(_encryptor);
            _pool.Return(this);
            _pool = null;
            _encryptor = null;
        }
    }
}