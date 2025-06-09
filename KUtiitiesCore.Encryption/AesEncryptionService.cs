using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Encryption
{
    /// <summary>
    /// Servicio de encriptación usando AES.
    /// </summary>
    sealed class AesEncryptionService : IEncryptionService
    {
        // La clave debe tener 16, 24 o 32 bytes para AES (128, 192 o 256 bits)
        private readonly byte[] _key;

        public AesEncryptionService(string key):this(Encoding.UTF8.GetBytes(key))
        {

        }
        /// <summary>
        /// Inicializa una nueva instancia del servicio de encriptación.
        /// </summary>
        /// <param name="key">Clave de encriptación en bytes.</param>
        public AesEncryptionService(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                throw new ArgumentException("La clave debe tener 16, 24 o 32 bytes.", nameof(key));

            _key = key;
        }

        /// <summary>
        /// Encripta el texto plano de forma asíncrona.
        /// </summary>
        /// <param name="plainText">Texto a encriptar.</param>
        /// <returns>Texto encriptado en Base64, donde los primeros bytes corresponden al IV.</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            using Aes aes = Aes.Create();
            aes.Key = _key;
            // Se genera un IV aleatorio para cada operación
            aes.GenerateIV();
            byte[] iv = aes.IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, iv);

            // Se utilizará un MemoryStream para escribir primero el IV y luego los datos encriptados
            using MemoryStream ms = new MemoryStream();
            // Escribimos el IV al principio
            ms.Write(iv, 0, iv.Length);

            // Usamos un CryptoStream para la encriptación
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write, leaveOpen: true))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                cs.Write(plainBytes, 0, plainBytes.Length);
            }

            // Se obtiene el arreglo de bytes resultante (IV + ciphertext) y se codifica en Base64
            byte[] encryptedBytes = ms.ToArray();
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Desencripta el texto cifrado de forma asíncrona.
        /// </summary>
        /// <param name="cipherText">Texto encriptado (Base64, con IV incluido).</param>
        /// <returns>Texto plano desencriptado.</returns>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));

            byte[] cipherCombined = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = _key;

            // Se extrae el IV (primeros bytes) según el tamaño del bloque
            int ivLength = aes.BlockSize / 8;
            byte[] iv = new byte[ivLength];
            Array.Copy(cipherCombined, 0, iv, 0, ivLength);
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            // Se crea un MemoryStream que apunta a la parte del ciphertext (excluyendo el IV)
            using MemoryStream msDecrypt = new MemoryStream(cipherCombined, ivLength, cipherCombined.Length - ivLength);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);

            // Se lee y retorna el texto desencriptado
            return srDecrypt.ReadToEnd();
        }
    }
}