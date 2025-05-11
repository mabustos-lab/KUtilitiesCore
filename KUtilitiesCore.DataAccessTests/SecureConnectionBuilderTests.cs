using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace KUtilitiesCore.DataAccess.Tests
{
    [TestClass()]
    public class SecureConnectionBuilderTests
    {
        [TestMethod()]
        public void IsValid_FalseTest()
        {
            SecureConnectionBuilder builder = new SecureConnectionBuilder();
            builder.InitialCatalog = string.Empty;
            Assert.IsFalse(builder.IsValid());
            
        }

        [TestMethod()]
        public void LoadTest()
        {
            SecureConnectionBuilder builder = new SecureConnectionBuilder();
            builder.Load();
        }


        [TestMethod()]
        public void SaveChangesTest()
        {
            SecureConnectionBuilder builder = new SecureConnectionBuilder
            {
                InitialCatalog = "SiomaxDB"
            };
            builder.SaveChanges();
        }

        [TestMethod()]
        public void TestConnectionTest()
        {
            SecureConnectionBuilder builder = new SecureConnectionBuilder
            {
                InitialCatalog = "SiomaxDB",
                ServerName = "localhost",
                 Encrypt = true,
                 TrustServerCertificate =true,
                  UserName= "sa",
                  Password = "@!Sefoil2908"
            };
            var result= builder.TestConnection();
            builder.ListDatabases();
            Assert.IsTrue(result.Sucess);
        }

        [TestMethod()]
        public void ListDatabasesTest()
        {
            SecureConnectionBuilder builder = new SecureConnectionBuilder
            {
                InitialCatalog = "SiomaxDB",
                ServerName = "localhost",
                Encrypt = true,
                TrustServerCertificate = true,
                UserName = "sa",
                Password = "@!Sefoil2908"
            };
            DataTable dt= builder.ListDatabases();
            Assert.IsTrue(dt!=null && dt.Rows.Count>0);
        }
    }
}