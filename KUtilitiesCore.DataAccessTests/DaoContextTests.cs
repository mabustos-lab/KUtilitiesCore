using KUtilitiesCore.Dal;
using KUtilitiesCore.Dal.ConnectionBuilder;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.Tests
{
    [TestClass()]
    public class DaoContextTests
    {
        private SecureConnectionBuilder builder;

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
                TrustServerCertificate = true,
                UserName = "sa",
                Password = "@!Sefoil2908"
            };
        }

        [TestMethod()]
        public void DaoContextTest()
        {
             DaoContext dao = new DaoContext(builder);
             var result= dao.ExecuteReader("Select * From ProfileScenario",null, System.Data.CommandType.Text);
            Assert.IsTrue(result.Tables.Count>0);
        }

        //[TestMethod()]
        //public void BeginTransactionTest()
        //{

        //}

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