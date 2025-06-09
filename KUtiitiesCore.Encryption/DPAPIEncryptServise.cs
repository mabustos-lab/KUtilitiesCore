using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#if windows
namespace KUtilitiesCore.Encryption
{
    sealed class DPAPIEncryptServise : IEncryptionService
    {
        public DataProtectionScope Scope { get; set; } = DataProtectionScope.CurrentUser;
        public string Decrypt(string cipherText)
        {
            byte[] bData = Convert.FromBase64String(cipherText);
            byte[] decryptedData = ProtectedData.Unprotect(bData, null, Scope);
            return Encoding.UTF8.GetString(decryptedData);
        }
                
        public string Encrypt(string plainText)
        {
            byte[] bData = Encoding.UTF8.GetBytes(plainText);

            byte[] encryptedData = ProtectedData.Protect(bData, null, Scope);
            return Convert.ToBase64String(encryptedData);
        }
    }
}
#endif
