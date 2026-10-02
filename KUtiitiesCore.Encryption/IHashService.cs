using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;

namespace KUtilitiesCore.Encryption
{
    /// <summary>
    /// Servicio de Hash para texto
    /// </summary>
    public interface IHashService
    {
        /// <summary>
        /// Genera el hash del texto plano aplicando la sal y las iteraciones configuradas.
        /// </summary>
        /// <param name="plainText">Texto plano a transformar.</param>
        /// <returns>El hash resultante listo para almacenarse y validarse posteriormente.</returns>
        string Hash(string plainText);
        /// <summary>
        /// Comprueba si un texto plano corresponde a un hash previamente generado por <see cref="Hash(string)"/>.
        /// </summary>
        /// <param name="plainText">Texto plano a verificar.</param>
        /// <param name="hashedText">Hash almacenado contra el que se compara.</param>
        /// <returns><see langword="true"/> si el texto genera el mismo hash; en caso contrario, <see langword="false"/>.</returns>
        bool IsValid(string plainText, string hashedText);
    }
}
