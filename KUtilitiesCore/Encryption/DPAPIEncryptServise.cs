using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace KUtilitiesCore.Encryption
{
#if windows
    sealed class DPAPIEncryptServise : IEncryptionService
    {
        
        public string Decrypt(string cipherText)
        {
            byte[] bData = Convert.FromBase64String(cipherText);
            byte[] decryptedData = ProtectedData.Unprotect(bData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedData);
        }
                
        public string Encrypt(string plainText)
        {
            byte[] bData = Encoding.UTF8.GetBytes(plainText);

            byte[] encryptedData = ProtectedData.Protect(bData, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }
    }
#endif
}
