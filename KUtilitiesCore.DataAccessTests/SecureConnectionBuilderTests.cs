﻿using KUtilitiesCore.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace KUtilitiesCore.DataAccess.Tests
{
    
    [TestClass()]
    public class SecureConnectionBuilderTests
    {
        [TestMethod()]
        public void IsValid_FalseTest()
        {
            // Excluir la prueba si se ejecuta en GitHub Actions (variable de entorno 'GITHUB_ACTIONS' == 'true')
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                Assert.Inconclusive("Esta prueba se omite en GitHub Actions.");
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Esta prueba solo se ejecuta en Windows.");
            }
            SecureConnectionBuilder builder = new SecureConnectionBuilder();
            builder.InitialCatalog = string.Empty;
            Assert.IsFalse(builder.IsValid());
            
        }

         [TestMethod()]
        public void SaveChangesTest()
        {
            // Excluir la prueba si se ejecuta en GitHub Actions (variable de entorno 'GITHUB_ACTIONS' == 'true')
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                Assert.Inconclusive("Esta prueba se omite en GitHub Actions.");
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Esta prueba solo se ejecuta en Windows.");
            }
            SecureConnectionBuilder builder = new SecureConnectionBuilder
            {
                InitialCatalog = "SiomaxDB"
            };
            builder.SaveChanges();
        }

        [TestMethod()]
        public void TestConnectionTest()
        {
            // Excluir la prueba si se ejecuta en GitHub Actions (variable de entorno 'GITHUB_ACTIONS' == 'true')
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                Assert.Inconclusive("Esta prueba se omite en GitHub Actions.");
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Esta prueba solo se ejecuta en Windows.");
            }
            SecureConnectionBuilder builder = new SecureConnectionBuilder
            {
                InitialCatalog = "SiomaxDB",
                ServerName = "localhost",
                Encrypt = true,
                TrustServerCertificate = true,
                UserName = "sa",
                Password = "@!Sefoil2908"
            };
            var result = builder.TestConnection();
            builder.ListDatabases();
            Assert.IsTrue(result.Sucess);
        }

        [TestMethod()]
        public void ListDatabasesTest()
        {
            // Excluir la prueba si se ejecuta en GitHub Actions (variable de entorno 'GITHUB_ACTIONS' == 'true')
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                Assert.Inconclusive("Esta prueba se omite en GitHub Actions.");
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Esta prueba solo se ejecuta en Windows.");
            }
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
