using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Encryption
{
    /// <summary>
    /// Define los métodos para un servicio de encriptación.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encripta un texto plano y devuelve el texto cifrado.
        /// </summary>
        /// <param name="plainText">El texto en claro que se desea encriptar.</param>
        /// <returns>El texto cifrado resultante.</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Desencripta un texto cifrado y devuelve el texto plano original.
        /// </summary>
        /// <param name="cipherText">El texto cifrado que se desea desencriptar.</param>
        /// <returns>El texto en claro resultante.</returns>
        string Decrypt(string cipherText);

    }
}
