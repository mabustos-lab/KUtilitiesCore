using System;
using System.Linq;
using System.Security.Cryptography;

namespace KUtilitiesCore.Encryption
{
    /// <summary>
    /// Permite generar un Hash de contraseña con sal aleatoria
    /// </summary>
    /// <remarks>
    /// Inicializa los parametros para realizar el Hash
    /// </remarks>
    /// <param name="useSaltHashOrder">Indica si se debe usar el orden Sal-Hash o Hashy-Salt</param>
    /// <param name="maximumSaltLength">Tamaño en Bytes de la Sal</param>
    /// <param name="Iterations">Número de iteraciones para derivar la clave</param>
    sealed class HashService(bool useSaltHashOrder = true, int maximumSaltLength = 32, int iterations = 10000) : IHashService
    {
        private readonly bool _useSaltHashOrder = useSaltHashOrder;
        private readonly int _maximumSaltLength = maximumSaltLength;
        private readonly int _iterations = iterations;

        public string Hash(string plainText)
        {
            return GetHashString(plainText, _maximumSaltLength, _useSaltHashOrder, _iterations);
        }

        public bool IsValid(string plainText, string hashedText)
        {
            return IsValidString(plainText, hashedText, _maximumSaltLength, _useSaltHashOrder, _iterations);
        }

        /// <summary>
        /// Permite generar un Hash de contraseña con sal aleatoria
        /// </summary>
        /// <param name="ValueToEncrypt">Cadena a encriptar</param>
        /// <param name="maximumSaltLength">Tamaño en Bytes de la Sal</param>
        /// <param name="UseSaltHashOrder">Indica si se debe usar el orden Sal + Hash, si es true o Hash + Salt si es false</param>
        /// <param name="Iterations">Número de iteraciones para derivar la clave</param>
        /// <returns></returns>
        static string GetHashString(string ValueToEncrypt,
            int maximumSaltLength, bool UseSaltHashOrder = true, int Iterations = 10000)
        {
            byte[] salt = SaltGenerator.GetSalt(maximumSaltLength);
            var pbkdf2 = new Rfc2898DeriveBytes(ValueToEncrypt, salt, Iterations, HashAlgorithmName.SHA256);
            //el hash tiene una longitud fija de 20 bytes
            byte[] hash = pbkdf2.GetBytes(20);
            //Para almacenar el valor hash ValueToEncrypt + sal
            byte[] hashBytes = new byte[20 + salt.Length];
            //copiar Salt + Hash, el orden puede variar
            if (UseSaltHashOrder)
            {
                Array.Copy(salt, 0, hashBytes, 0, salt.Length);
                Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);
            }
            else
            {
                Array.Copy(hash, 0, hashBytes, 0, hash.Length);
                Array.Copy(salt, 0, hashBytes, hash.Length, salt.Length);
            }

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Compara el un texto con su version encryptada
        /// </summary>
        /// <param name="NoEncrypedString">Texto sin encriptar</param>
        /// <param name="HashedString">Texto encriptado</param>
        /// <param name="maximumSaltLength">Longitud de la sal</param>
        /// <param name="UseSaltHashOrder">Indica si se debe usar el orden Sal-Hash</param>
        /// <param name="Iterations">Número de iteraciones para derivar la clave</param>
        /// <returns></returns>
        static bool IsValidString(string NoEncrypedString, string HashedString,
            int maximumSaltLength, bool UseSaltHashOrder = true, int Iterations = 10000)
        {
            byte[] hashBytes = Convert.FromBase64String(HashedString);
            byte[] salt = new byte[maximumSaltLength];
            Array.Copy(hashBytes, UseSaltHashOrder ? 0 : 20, salt, 0, maximumSaltLength);
            var pbkdf2 = new Rfc2898DeriveBytes(NoEncrypedString, salt, Iterations, HashAlgorithmName.SHA256);
            //El hash tiene 20 bytes de longitud fija.
            byte[] hash = pbkdf2.GetBytes(20);
            if (UseSaltHashOrder)
            {
                return hashBytes.Skip(maximumSaltLength).SequenceEqual(hash);
            }
            return hashBytes.Take(hash.Length).SequenceEqual(hash);
        }
    }
}
