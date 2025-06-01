using System.Security.Cryptography;

namespace KUtilitiesCore.Encryption
{
    /// <summary>
    /// Clase generadora de valores aleatorios usados como sal en procesos de encriptación.
    /// </summary>
    /// <remarks>
    /// La clase utiliza RandomNumberGenerator para generar bytes criptográficamente seguros.
    /// </remarks>
    static class SaltGenerator
    {
        /// <summary>
        /// Límite de longitud máximo para el arreglo de bytes generado.
        /// </summary>
        private static readonly int SaltLengthLimit = 32;

        /// <summary>
        /// Genera un arreglo de bytes aleatorios usando el límite predeterminado de longitud.
        /// </summary>
        /// <returns>Arreglo de bytes aleatorios.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce si no se puede generar el arreglo de bytes.
        /// </exception>
        public static byte[] GetSalt()
        {
            return GetSalt(SaltLengthLimit);
        }

        /// <summary>
        /// Genera un arreglo de bytes aleatorios con una longitud máxima especificada.
        /// </summary>
        /// <param name="maximumSaltLength">Longitud máxima del arreglo de bytes generado.</param>
        /// <returns>Arreglo de bytes aleatorios con longitud entre 4 y maximumSaltLength.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se produce si maximumSaltLength no está en el rango permitido.
        /// </exception>
        public static byte[] GetSalt(int maximumSaltLength)
        {
            var salt = new byte[maximumSaltLength];
#if NET6_0_OR_GREATER
            // Llena el array de bytes con números aleatorios
            RandomNumberGenerator.Fill(salt);
#else
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Llena el array de bytes con números aleatorios
                rng.GetBytes(salt);
            }
#endif
            return salt;
        }


#if NET48
        /// <summary>
        /// Provee un <see cref="Random"/> thread-safe que puede ser usado concurrentemente en cualquier hilo
        /// Provides a thread-safe System.Random instance that may be used concurrently from any thread.
        /// </summary>
        /// <returns></returns>
        public static readonly ThreadLocal<Random> Random = new(() =>
        {
            // Se genera una semilla criptográficamente segura para cada instancia.
            using var crypto = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] seedBytes = new byte[4];
            crypto.GetBytes(seedBytes);
            int seed = BitConverter.ToInt32(seedBytes, 0);
            return new Random(seed);
        });
#endif
    }
}