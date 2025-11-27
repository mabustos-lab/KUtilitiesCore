using KUtilitiesCore.Dal;
using KUtilitiesCore.Dal.ConnectionBuilder;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.Tests
{
    [TestClass()]
    public class DaoContextTests
    {
        private SecureConnectionBuilder? builder;
        internal class ActiveTypeModel
        {
            public int ID { get; set; }
            public string Context { get; set; }
            public string Description { get; set; }
        }
        [TestInitialize]
        public void Initilize()
        {
#if NET8_0_OR_GREATER
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
#endif
            builder = new SecureConnectionBuilder
            {
                InitialCatalog = "SiomaxDB",
                ServerName = "localhost",
                Encrypt = true,
                IntegratedSecurity =true,
                TrustServerCertificate = true
            };
        }
        
        [TestMethod()]
        public void DaoContextTest()
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
            using DaoContext dao = new DaoContext(builder);
            DataSet ds =new DataSet(); 
            dao.FillDataSet("Select * From ProfileScenario", ds, "ProfileScenario");
            
            Assert.IsTrue(ds.Tables.Count>0);
        }

        [TestMethod()]
        public void ExecuteReader_WithResult_Test()
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
            
            using DaoContext dao = new(builder,metrics: new Telemetry.LoggerMetrics());
            var converter= Helpers.DataReaderConverter.Create()
                .WithResult<ActiveTypeModel>()
                .SetStrictMapping(true);
            var result= dao.ExecuteReader("Select * From ActiveType", converter, commandType: CommandType.Text);
            Assert.IsTrue(result.HasResultsets);
        }

        //[TestMethod()]
        //public void CreateAdapterTest()
        //{

        //}

        //[TestMethod()]
        //public void CreateCommandTest()
        //{

        //}

        //[TestMethod()]
        //public void CreateCommandBuilderTest()
        //{

        //}

        //[TestMethod()]
        //public void CreateParameterCollectionTest()
        //{

        //}

        //[TestMethod()]
        //public void DatabaseExistsTest()
        //{

        //}

        //[TestMethod()]
        //public void DisposeTest()
        //{

        //}

        //[TestMethod()]
        //public void ExecuteNonQueryTest()
        //{

        //}

        //[TestMethod()]
        //public void ExecuteNonQueryAsyncTest()
        //{

        //}

        //[TestMethod()]
        //public void ExecuteReaderTest()
        //{

        //}

        //[TestMethod()]
        //public void ExecuteReaderAsyncTest()
        //{

        //}

        //[TestMethod()]
        //public void FillDataSetTest()
        //{

        //}

        //[TestMethod()]
        //public void ScalarTest()
        //{

        //}

        //[TestMethod()]
        //public void ScalarAsyncTest()
        //{

        //}

        //[TestMethod()]
        //public void UpdateDataSetTest()
        //{

        //}
    }
}