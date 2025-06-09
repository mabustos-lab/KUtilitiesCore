using System;
using System.Linq;
using System.Security.Cryptography;

namespace KUtilitiesCore.Encryption
{
    public static class FactoryEncryptionService
    {
        /// <summary>
        /// Crea el servicio para generar un Hash de contraseña con sal aleatoria
        /// </summary>
        /// <param name = "useSaltHashOrder" > Indica si se debe usar el orden Sal-Hash o Hashy-Salt</param>
        /// <param name="maximumSaltLength">Tamaño en Bytes de la Sal</param>
        /// <param name="Iterations">Número de iteraciones para derivar la clave</param>
        /// <returns></returns>
        public static IHashService GetHashServise(bool useSaltHashOrder = true, int maximumSaltLength = 32, int iterations = 10000)
        {
            return new HashService(useSaltHashOrder, maximumSaltLength, iterations);
        }
        /// <summary>
        /// Implementación básica de IEncryptionService.
        /// </summary>
        /// <returns></returns>
        public static IEncryptionService GetBase64EncryptionService()
        {
            return new Base64EncryptionService();
        }
#if WINDOWS
        /// <summary>
        /// Implementación Data Protection API
        /// </summary>
        /// <returns></returns>
        public static IEncryptionService GetDPAPIEncryptionService()
        {
            return new DPAPIEncryptServise();
        }
#endif
        /// <summary>
        /// Servicio de encriptación usando AES.
        /// </summary>
        /// <param name="key">La clave debe tener 16, 24 o 32 bytes para AES (128, 192 o 256 bits)</param>
        /// <returns></returns>
        public static IEncryptionService GetAesEncryptionService(string key)
        {
            return new AesEncryptionService(key);
        }
    }
}
