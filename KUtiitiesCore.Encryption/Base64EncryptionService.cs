using System;
using System.Linq;
using System.Text;

namespace KUtilitiesCore.Encryption
{
    /// <summary>
    /// Implementación básica de IEncryptionService.
    /// </summary>
    sealed class Base64EncryptionService : IEncryptionService
    {
        public string Encrypt(string plainText)
        {
            // Ejemplo básico de encriptación (no usar en producción)
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainBytes);
        }

        public string Decrypt(string cipherText)
        {
            // Ejemplo básico de desencriptación (no usar en producción)
            var cipherBytes = Convert.FromBase64String(cipherText);
            return Encoding.UTF8.GetString(cipherBytes);
        }
    }
}
