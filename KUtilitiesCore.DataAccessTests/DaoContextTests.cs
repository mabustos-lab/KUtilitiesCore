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
#if NET8_0_OR_GREATER
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
#endif
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
            // Excluir la prueba si se ejecuta en GitHub Actions (variable de entorno 'GITHUB_ACTIONS' == 'true')
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                Assert.Inconclusive("Esta prueba se omite en GitHub Actions.");
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Esta prueba solo se ejecuta en Windows.");
            }
            // Arrange
            var mockDataReader = new Moq.Mock<DbDataReader>();

            // SetupSequence for Read() for all result sets
            mockDataReader.SetupSequence(dr => dr.Read())
                // UserDto Result Set (2 rows)
                .Returns(true)
                .Returns(true)
                .Returns(false)
                // InvoiceDto Result Set (2 rows)
                .Returns(true)
                .Returns(true)
                .Returns(false)

                // DataTable Result Set (1 row)
                .Returns(true)
                .Returns(false)
                .Returns(false); // No more result sets



            // SetupSequence for NextResult()
            mockDataReader.SetupSequence(dr => dr.NextResult())
                .Returns(true) // Move from UserDto to InvoiceDto
                .Returns(true) // Move from InvoiceDto to DataTable
                .Returns(false); // No more result sets

            // SetupSequence for FieldCount
            mockDataReader.SetupSequence(dr => dr.FieldCount)
                .Returns(2) // UserDto has 2 fields
                .Returns(3) // InvoiceDto has 3 fields
                .Returns(1); // DataTable has 1 field

            // Setup GetName, GetFieldType, GetOrdinal for UserDto, InvoiceDto, and DataTable
            // This needs to be sequenced or handled dynamically as GetName/GetFieldType changes per result set

            mockDataReader.SetupSequence(dr => dr.GetName(It.IsAny<int>()))
                // For UserDto (2 fields)
                .Returns("Id").Returns("Name")
                // For InvoiceDto (3 fields)
                .Returns("Id").Returns("UserId").Returns("Amount")
                // For DataTable (1 field)
                .Returns("Column1");

            mockDataReader.SetupSequence(dr => dr.GetFieldType(It.IsAny<int>()))
                // For UserDto (2 fields)
                .Returns(typeof(int)).Returns(typeof(string))
                // For InvoiceDto (3 fields)
                .Returns(typeof(int)).Returns(typeof(int)).Returns(typeof(decimal))
                // For DataTable (1 field)
                .Returns(typeof(string));

            mockDataReader.SetupSequence(dr => dr.GetOrdinal(It.IsAny<string>()))
                // For UserDto
                .Returns(0) // Id
                .Returns(1) // Name
                            // For InvoiceDto
                .Returns(1) // Id
                .Returns(1) // UserId
                .Returns(2) // Amount
                            // For DataTable
                .Returns(0); // Column1

            mockDataReader.Setup(dr => dr.GetValue(0)).Returns(1); // User 1.Id
            mockDataReader.Setup(dr => dr.GetValue(1)).Returns("User1"); // User 1.Name


            // No GetValue(2) setup needed as there are only 2 columns (for UserDto)

            using DaoContext dao = new(builder, metrics: new Telemetry.LoggerMetrics());

            var converter = Helpers.DataReaderConverter.Create()
                .WithResult<UserDto>()      // First select is Users
                .WithResult<InvoiceDto>()   // Second select is Invoices
                .WithDefaultDataTable();    // If there is a third, let it be DataTable

            // Act
            var result = ((IDaoContext)dao).ExecuteReader("sp_GetData", converter, commandType: CommandType.StoredProcedure, dbDataReader: mockDataReader.Object);

            // Assert
            Assert.IsTrue(result.HasResultsets);
            Assert.AreEqual(3, result.ResultSetCount);
            var users = result.GetResult<UserDto>().ToList();
            Assert.IsNotNull(users);
            Assert.HasCount(2, users);
            Assert.AreEqual(1, users[0].Id);
            Assert.AreEqual("User1", users[0].Name);
        }
    }
}
