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
    public class HashServiceTests
    {
        IHashService servise;
        [TestInitialize()]
        public void Setup()
        {
            servise =FactoryEncryptionService.GetHashServise();
        }
        [TestMethod()]
        public void HashTest()
        {
            string hashedString= servise.Hash("Prueba");
            Assert.IsTrue(!hashedString.Equals("Prueba"));
        }

        [TestMethod()]
        public void IsValidTest()
        {
            string hashedString = servise.Hash("Prueba");
            Assert.IsTrue(servise.IsValid("Prueba", hashedString));
        }
    }
}