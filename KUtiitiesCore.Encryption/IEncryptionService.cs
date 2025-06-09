using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Encryption
{
    /// <summary>
    /// Servicio de encriptación.
    /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string plainText);

        string Decrypt(string cipherText);
    }
}
