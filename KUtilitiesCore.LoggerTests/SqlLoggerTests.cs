using KUtilitiesCore.Logger;
using KUtilitiesCore.Logger.Options;
using KUtilitiesCore.Logger.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.SqlClient;
using System;
using Microsoft.Extensions.Logging;

namespace KUtilitiesCore.LoggerTests
{
    [TestClass]
    public class SqlLoggerTests
    {
        private const string ConnectionString = "Server=localhost;Database=KUtilitiesLogTest;Integrated Security=True;TrustServerCertificate=True;";
        private const string TableName = "TestLogs";
        private SqlLoggerOptions _options;
        private SqlLoggerServiceProvider _provider;

        [TestInitialize]
        public void Setup()
        {
            _options = new SqlLoggerOptions
            {
                ConnectionString = ConnectionString,
                TableName = TableName,
                AutoCreateTable = true,
                ApplicationName = "UnitTest"
            };
            _provider = new SqlLoggerServiceProvider(_options);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [Ignore("Requiere SQL Server local configurado")]
        public void CreateLogger_And_LogMessage_ToSql()
        {
            var logger = _provider.CreateLogger<SqlLoggerTests>();
            Assert.IsNotNull(logger);
            string msgeTest = "Mensaje de prueba desde Test Unitario";
            try
            {
                logger.LogInformation(msgeTest, null);
                
                // Verificar si se insertó (opcional, requiere consulta SQL)
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand($"SELECT COUNT(*) FROM dbo.{TableName} WHERE Message LIKE '%{msgeTest}%'", connection);
                    int count = (int)cmd.ExecuteScalar();
                    Assert.IsTrue(count > 0, "El mensaje no se encontró en la base de datos.");
                }
            }
            catch (SqlException ex)
            {
                Assert.Inconclusive($"No se pudo conectar a SQL Server: {ex.Message}");
            }
        }

        [TestMethod]
        public void Provider_Name_IsCorrect()
        {
            Assert.AreEqual("SqlLoggerServiceProvider", _provider.Name);
        }

        [TestMethod]
        public void Provider_Throws_If_ConnectionString_Empty()
        {
            var options = new SqlLoggerOptions { ConnectionString = "" };
            try
            {
                new SqlLoggerServiceProvider(options);
                Assert.Fail("Debería haber lanzado ArgumentException");
            }
            catch (ArgumentException)
            {
                // Éxito
            }
        }
    }
}
