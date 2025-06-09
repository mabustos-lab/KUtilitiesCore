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
        string Hash(string plainText);
        bool IsValid(string plainText, string hashedText);
    }
}
