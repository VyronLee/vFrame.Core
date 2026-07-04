// ------------------------------------------------------------
//         File: EncryptorPool.cs
//        Brief: Factory for renting encryptor instances
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
        /// <summary>
        ///     No-op; encryptors are no longer pooled, so there is nothing to
        ///     initialize at singleton creation.
        /// </summary>
        protected override void OnCreate() { }

        /// <summary>
        ///     Rents a new encryptor of the specified type. The returned instance
        ///     is owned by the caller and must be disposed (e.g. via <c>using</c>)
        ///     when finished; it is not reused.
        /// </summary>
        /// <param name="encryptorType">The type of encryptor to rent.</param>
        /// <returns>A fresh <see cref="IEncryptor" /> instance ready for use.</returns>
        /// <exception cref="System.ArgumentException">Thrown when an unsupported encryptor type is specified.</exception>
        public IEncryptor Rent(EncryptorType encryptorType) {
            Encryptor encryptor = null;
            switch (encryptorType) {
                case EncryptorType.Plain:
                    encryptor = new PlainEncryptor();
                    break;
                case EncryptorType.Xor:
                    encryptor = new XOREncryptor();
                    break;
                case EncryptorType.AesGcm:
                    encryptor = new AesGcmEncryptor();
                    break;
                default:
                    ThrowHelper.ThrowUnsupportedEnum(encryptorType);
                    break;
            }

            if (null == encryptor) {
                return null;
            }

            encryptor.Create();
            return encryptor;
        }
    }
}
