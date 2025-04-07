using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Encryption.Tests
{
    [TestClass()]
    public class AesEncryptionServiceTests
    {
        IEncryptionService eServise;

        [TestInitialize()]
        public void Setup()
        {
            eServise = FactoryEncryptionService.GetAesEncryptionService("Prueba1234567890");
        }

        [TestMethod()]
        public void EncryptTest()
        {
            try
            {
                string encryptedValue = eServise.Encrypt("Valor");
                Assert.IsTrue(!encryptedValue.Equals("Valor"));
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void DecryptTesT()
        {
            try
            {
                string encryptedValue = eServise.Encrypt("Valor");
                Assert.IsTrue(eServise.Decrypt(encryptedValue).Equals("Valor"));
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}