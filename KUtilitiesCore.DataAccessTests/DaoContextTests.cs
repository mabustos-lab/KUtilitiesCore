using Moq;
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
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
            builder = new SecureConnectionBuilder
            {
                InitialCatalog = "SiomaxDB",
                ServerName = "localhost",
                Encrypt = true,
                IntegratedSecurity = true,
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
            Logger.LoggerServiceFactory lf = new Logger.LoggerServiceFactory();
            Logger.Options.LoggerOptions lo = new Logger.Options.LoggerOptions();
            lo.MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
            lf.AddProvider(new Logger.Providers.DebugLoggerServiceProvider(lo));
            using DaoContext dao = new DaoContext(builder, lf.GetLogger<DaoContext>());
            DataSet ds = new DataSet();
            dao.FillDataSet("Select * From ProfileScenario", ds, "ProfileScenario");

            Assert.IsTrue(ds.Tables.Count > 0);
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

            using DaoContext dao = new(builder, metrics: new Telemetry.LoggerMetrics());
            var converter = Helpers.DataReaderConverter.Create()
                .WithResult<ActiveTypeModel>()
                .SetStrictMapping(true);
            var result = dao.ExecuteReader("Select * From ActiveType", converter, commandType: CommandType.Text);
            Assert.IsTrue(result.HasResultsets);
            var l = result.GetResult<ActiveTypeModel>().ToList();
            Assert.IsGreaterThan(0, l.Count);
        }

        [TestMethod()]
        public void ExecuteReader_WithDataTable_Test()
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

            using DaoContext dao = new(builder, metrics: new Telemetry.LoggerMetrics());
            var converter = Helpers.DataReaderConverter.Create().WithDefaultDataTable()
                .SetStrictMapping(true);
            var result = dao.ExecuteReader("Select * From ActiveType", converter, commandType: CommandType.Text);
            Assert.IsTrue(result.HasResultsets);
            var l = result.GetResult<ActiveTypeModel>().ToList();
            Assert.IsGreaterThan(0, l.Count);
        }

        // Define simple DTOs for testing multiple result sets
        internal class UserDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        internal class InvoiceDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public decimal Amount { get; set; }
        }

        [TestMethod()]

        public void ExecuteReader_MultipleResultSets_Test()
        {
            
            // Arrange
            var mockDataReader = new Moq.Mock<DbDataReader>() { CallBase = true };

            int currentResultSet = 0;
            int currentRow = -1;
            int[] rowsPerResultSet = { 2, 2, 1 };

            mockDataReader.Setup(dr => dr.Read()).Returns(() =>
            {
                currentRow++;
                bool hasMore = currentResultSet < rowsPerResultSet.Length && currentRow < rowsPerResultSet[currentResultSet];
                return hasMore;
            });

            mockDataReader.Setup(dr => dr.NextResult()).Returns(() =>
            {
                currentResultSet++;
                currentRow = -1;
                return currentResultSet < rowsPerResultSet.Length;
            });

            mockDataReader.Setup(dr => dr.FieldCount).Returns(() =>
            {
                if (currentResultSet == 0) return 2;
                if (currentResultSet == 1) return 3;
                if (currentResultSet == 2) return 1;
                return 0;
            });

            mockDataReader.Setup(dr => dr.GetName(It.IsAny<int>())).Returns((int i) =>
            {
                if (currentResultSet == 0) return i == 0 ? "Id" : "Name";
                if (currentResultSet == 1) return i == 0 ? "Id" : i == 1 ? "UserId" : "Amount";
                if (currentResultSet == 2) return "Column1";
                return string.Empty;
            });

            mockDataReader.Setup(dr => dr.GetFieldType(It.IsAny<int>())).Returns((int i) =>
            {
                if (currentResultSet == 0) return i == 0 ? typeof(int) : typeof(string);
                if (currentResultSet == 1) return i == 0 ? typeof(int) : i == 1 ? typeof(int) : typeof(decimal);
                if (currentResultSet == 2) return typeof(string);
                return typeof(object);
            });

            mockDataReader.Setup(dr => dr.GetValue(It.IsAny<int>())).Returns((int i) =>
            {
                if (currentResultSet == 0)
                {
                    if (currentRow == 0) return i == 0 ? (object)1 : (object)"User1";
                    if (currentRow == 1) return i == 0 ? (object)2 : (object)"User2";
                }
                else if (currentResultSet == 1)
                {
                    if (currentRow == 0) return i == 0 ? (object)101 : i == 1 ? (object)1 : (object)50.5m;
                    if (currentRow == 1) return i == 0 ? (object)102 : i == 1 ? (object)2 : (object)150.0m;
                }
                else if (currentResultSet == 2)
                {
                    if (currentRow == 0) return (object)"Value1";
                }
                return DBNull.Value;
            });

            // Explicitly setup GetValues to avoid any CallBase issues
            mockDataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>())).Returns((object[] values) =>
            {
                int fieldCount = mockDataReader.Object.FieldCount;
                int count = Math.Min(values.Length, fieldCount);
                for (int i = 0; i < count; i++)
                {
                    values[i] = mockDataReader.Object.GetValue(i);
                }
                return count;
            });

            using DaoContext dao = new(builder, metrics: new Telemetry.LoggerMetrics());

            var converter = Helpers.DataReaderConverter.Create()
                .WithResult<UserDto>()      // First select is Users
                .WithResult<InvoiceDto>()   // Second select is Invoices
                .WithDefaultDataTable();    // If there is a third, let it be DataTable

            // Act
            var result = dao.ExecuteReaderCore("sp_GetData", converter, 
                commandType: CommandType.StoredProcedure, dbDataReader: mockDataReader.Object);

            // Assert
            Assert.IsTrue(result.HasResultsets);
            Assert.AreEqual(3, result.ResultSetCount);
            
            var users = result.GetResult<UserDto>().ToList();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count);
            Assert.AreEqual(1, users[0].Id);
            Assert.AreEqual("User1", users[0].Name);
            Assert.AreEqual(2, users[1].Id);
            Assert.AreEqual("User2", users[1].Name);

            var invoices = result.GetResult<InvoiceDto>(1).ToList();
            Assert.IsNotNull(invoices);
            Assert.AreEqual(2, invoices.Count);
            Assert.AreEqual(101, invoices[0].Id);
            Assert.AreEqual(50.5m, invoices[0].Amount);

            var dt = result.GetDataTable(2);
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual("Value1", dt.Rows[0][0]);
        }
    }
}
