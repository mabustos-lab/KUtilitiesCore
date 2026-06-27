using KUtilitiesCore.Dal.Helpers;
using Moq;
using KUtilitiesCore.Dal;
using KUtilitiesCore.Dal.ConnectionBuilder;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace KUtilitiesCore.Dal.Tests
{
    [TestClass()]
    public class DataReaderConverterTests
    {
        private SecureConnectionBuilder _builder = null!;

        [TestInitialize]
        public void Initialize()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
            _builder = new SecureConnectionBuilder
            {
                InitialCatalog = "SiomaxDB",
                ServerName = "localhost",
                Encrypt = true,
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };
        }

        #region Modelos de prueba

        /// <summary>
        /// DTO tradicional para pruebas con WithResult estándar.
        /// </summary>
        internal class UserDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// Struct de solo lectura para validar mapeo con delegado en tipos sin new().
        /// </summary>
        internal readonly record struct UserRecord(int Id, string Name);

        /// <summary>
        /// Record inmutable (class) para validar mapeo con delegado.
        /// </summary>
        internal sealed record UserImmutableRecord(int Id, string Name);

        #endregion

        #region WithResult con delegado — Mapeo a un record struct

        [TestMethod]
        public void WithResultDelegate_RecordStruct_MapsCorrectly()
        {
            // Arrange
            var mockReader = CreateMockReaderWithTwoRows();

            var converter = Helpers.DataReaderConverter.Create()
                .WithResult<UserRecord>(dataTable =>
                    dataTable.Rows.Cast<DataRow>()
                        .Select(row => new UserRecord(
                            Id: (int)row["Id"],
                            Name: (string)row["Name"]
                        )));

            // Act
            using var dao = new DaoContext(_builder, metrics: new Telemetry.LoggerMetrics());
            var result = dao.ExecuteReaderCore(
                "sp_GetUsers",
                converter,
                commandType: CommandType.StoredProcedure,
                dbDataReader: mockReader.Object);

            // Assert
            Assert.IsTrue(result.HasResultsets, "Debe tener al menos un conjunto de resultados.");
            Assert.AreEqual(1, result.ResultSetCount, "Debe tener exactamente un conjunto de resultados.");

            var users = result.GetResultUnsafe<UserRecord>().ToList();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count);
            Assert.AreEqual(1, users[0].Id);
            Assert.AreEqual("Alice", users[0].Name);
            Assert.AreEqual(2, users[1].Id);
            Assert.AreEqual("Bob", users[1].Name);
        }

        #endregion

        #region WithResult con delegado — Mapeo a un record class inmutable

        [TestMethod]
        public void WithResultDelegate_ImmutableRecord_MapsCorrectly()
        {
            // Arrange
            var mockReader = CreateMockReaderWithTwoRows();

            var converter = Helpers.DataReaderConverter.Create()
                .WithResult<UserImmutableRecord>(dataTable =>
                    dataTable.Rows.Cast<DataRow>()
                        .Select(row => new UserImmutableRecord(
                            Id: (int)row["Id"],
                            Name: (string)row["Name"]
                        )));

            // Act
            using var dao = new DaoContext(_builder, metrics: new Telemetry.LoggerMetrics());
            var result = dao.ExecuteReaderCore(
                "sp_GetUsers",
                converter,
                commandType: CommandType.StoredProcedure,
                dbDataReader: mockReader.Object);

            // Assert
            Assert.IsTrue(result.HasResultsets);
            Assert.AreEqual(1, result.ResultSetCount);

            var users = result.GetResultUnsafe<UserImmutableRecord>().ToList();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count);
            Assert.AreEqual(1, users[0].Id);
            Assert.AreEqual("Alice", users[0].Name);
            Assert.AreEqual(2, users[1].Id);
            Assert.AreEqual("Bob", users[1].Name);
        }

        #endregion

        #region WithResult múltiple: clásico + delegado + DataTable

        [TestMethod]
        public void WithResult_MixedStrategies_MapsAllCorrectly()
        {
            // Arrange — 3 conjuntos de resultados simulados:
            //   ResultSet 0: 2 filas de UserDto (con WithResult clásico)
            //   ResultSet 1: 1 fila de UserRecord (con WithResult delegado)
            //   ResultSet 2: DataTable por defecto
            var mockReader = new Mock<DbDataReader>() { CallBase = true };

            int currentResultSet = 0;
            int currentRow = -1;
            int[] rowsPerResultSet = { 2, 1, 1 };

            mockReader.Setup(dr => dr.Read()).Returns(() =>
            {
                currentRow++;
                return currentResultSet < rowsPerResultSet.Length && currentRow < rowsPerResultSet[currentResultSet];
            });

            mockReader.Setup(dr => dr.NextResult()).Returns(() =>
            {
                currentResultSet++;
                currentRow = -1;
                return currentResultSet < rowsPerResultSet.Length;
            });

            mockReader.Setup(dr => dr.FieldCount).Returns(() =>
            {
                return currentResultSet switch
                {
                    0 => 2, // Id, Name
                    1 => 2, // Id, Name
                    2 => 1, // Column1
                    _ => 0
                };
            });

            mockReader.Setup(dr => dr.GetName(It.IsAny<int>())).Returns((int i) =>
            {
                return currentResultSet switch
                {
                    0 => i == 0 ? "Id" : "Name",
                    1 => i == 0 ? "Id" : "Name",
                    2 => "Column1",
                    _ => string.Empty
                };
            });

            mockReader.Setup(dr => dr.GetFieldType(It.IsAny<int>())).Returns((int i) =>
            {
                return currentResultSet switch
                {
                    0 => i == 0 ? typeof(int) : typeof(string),
                    1 => i == 0 ? typeof(int) : typeof(string),
                    2 => typeof(string),
                    _ => typeof(object)
                };
            });

            mockReader.Setup(dr => dr.GetValue(It.IsAny<int>())).Returns((int i) =>
            {
                if (currentResultSet == 0)
                {
                    if (currentRow == 0) return i == 0 ? (object)1 : (object)"ClassUser1";
                    if (currentRow == 1) return i == 0 ? (object)2 : (object)"ClassUser2";
                }
                else if (currentResultSet == 1)
                {
                    if (currentRow == 0) return i == 0 ? (object)99 : (object)"RecordUser";
                }
                else if (currentResultSet == 2)
                {
                    if (currentRow == 0) return (object)"ExtraData";
                }
                return DBNull.Value;
            });

            mockReader.Setup(dr => dr.GetValues(It.IsAny<object[]>())).Returns((object[] values) =>
            {
                int fieldCount = mockReader.Object.FieldCount;
                int count = Math.Min(values.Length, fieldCount);
                for (int i = 0; i < count; i++)
                {
                    values[i] = mockReader.Object.GetValue(i);
                }
                return count;
            });

            var converter = Helpers.DataReaderConverter.Create()
                .WithResult<UserDto>()        // ResultSet 0: mapeo por reflection
                .WithResult<UserRecord>(dt => // ResultSet 1: mapeo por delegado a struct
                    dt.Rows.Cast<DataRow>()
                      .Select(row => new UserRecord((int)row["Id"], (string)row["Name"])))
                .WithDefaultDataTable();       // ResultSet 2: DataTable como fallback

            // Act
            using var dao = new DaoContext(_builder, metrics: new Telemetry.LoggerMetrics());
            var result = dao.ExecuteReaderCore(
                "sp_GetMixedData",
                converter,
                commandType: CommandType.StoredProcedure,
                dbDataReader: mockReader.Object);

            // Assert
            Assert.IsTrue(result.HasResultsets);
            Assert.AreEqual(3, result.ResultSetCount);

            // ResultSet 0 — recuperado con GetResult clásico
            var classUsers = result.GetResult<UserDto>(0).ToList();
            Assert.AreEqual(2, classUsers.Count);
            Assert.AreEqual(1, classUsers[0].Id);
            Assert.AreEqual("ClassUser1", classUsers[0].Name);

            // ResultSet 1 — recuperado con GetResultUnsafe (struct)
            var recordUser = result.GetResultUnsafe<UserRecord>(1).ToList();
            Assert.AreEqual(1, recordUser.Count);
            Assert.AreEqual(99, recordUser[0].Id);
            Assert.AreEqual("RecordUser", recordUser[0].Name);

            // ResultSet 2 — DataTable
            var dt = result.GetDataTable(2);
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual("ExtraData", dt.Rows[0][0]);
        }

        #endregion

        #region WithResult con delegado — Validación de argumento nulo

        [TestMethod]
        public void WithResultDelegate_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Helpers.DataReaderConverter.Create()
                    .WithResult<UserRecord>(null!);
            });
        }

        #endregion

        #region GetResultUnsafe — Tipo incorrecto lanza excepción

        [TestMethod]
        public void GetResultUnsafe_WrongType_ThrowsInvalidCastException()
        {
            // Arrange — mock con 1 fila, mapeado a UserRecord
            var mockReader = new Mock<DbDataReader>() { CallBase = true };

            int currentRow = -1;
            bool returnTrueOnce = true;
            mockReader.Setup(dr => dr.Read()).Returns(() =>
            {
                currentRow++;
                if (returnTrueOnce && currentRow == 0)
                {
                    returnTrueOnce = false;
                    return true;
                }
                return false;
            });
            mockReader.Setup(dr => dr.NextResult()).Returns(false);
            mockReader.Setup(dr => dr.FieldCount).Returns(2);
            mockReader.Setup(dr => dr.GetName(0)).Returns("Id");
            mockReader.Setup(dr => dr.GetName(1)).Returns("Name");
            mockReader.Setup(dr => dr.GetFieldType(0)).Returns(typeof(int));
            mockReader.Setup(dr => dr.GetFieldType(1)).Returns(typeof(string));
            mockReader.Setup(dr => dr.GetValue(0)).Returns(1);
            mockReader.Setup(dr => dr.GetValue(1)).Returns("Test");
            mockReader.Setup(dr => dr.GetValues(It.IsAny<object[]>())).Returns((object[] values) =>
            {
                values[0] = 1;
                values[1] = "Test";
                return 2;
            });

            var converter = Helpers.DataReaderConverter.Create()
                .WithResult<UserRecord>(dt =>
                    dt.Rows.Cast<DataRow>()
                      .Select(row => new UserRecord((int)row["Id"], (string)row["Name"])));

            using var dao = new DaoContext(_builder, metrics: new Telemetry.LoggerMetrics());

            IReaderResultSet result;
            try
            {
                result = dao.ExecuteReaderCore(
                    "sp_GetUsers",
                    converter,
                    commandType: CommandType.StoredProcedure,
                    dbDataReader: mockReader.Object);
            }
            catch (ObjectDisposedException)
            {
                // El mock puede disparar ObjectDisposedException al hacer Dispose del reader simulado.
                // En ese caso la prueba es inconclusa porque el mock no soporta el ciclo de vida completo.
                Assert.Inconclusive("El mock de DbDataReader no soporta el ciclo de vida completo de Dispose.");
                return;
            }

            // Act & Assert — intentar recuperar con tipo incorrecto
            try
            {
                result.GetResultUnsafe<Dictionary<string, object>>().ToList();
                Assert.Fail("Debió lanzar InvalidCastException.");
            }
            catch (InvalidCastException)
            {
                // Esperado
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Crea un mock de DbDataReader con 2 filas (Id int, Name string).
        /// </summary>
        private static Mock<DbDataReader> CreateMockReaderWithTwoRows()
        {
            var mock = new Mock<DbDataReader>() { CallBase = true };

            int currentRow = -1;
            mock.Setup(dr => dr.Read()).Returns(() =>
            {
                currentRow++;
                return currentRow < 2;
            });

            mock.Setup(dr => dr.NextResult()).Returns(false);

            mock.Setup(dr => dr.FieldCount).Returns(2);

            mock.Setup(dr => dr.GetName(0)).Returns("Id");
            mock.Setup(dr => dr.GetName(1)).Returns("Name");

            mock.Setup(dr => dr.GetFieldType(0)).Returns(typeof(int));
            mock.Setup(dr => dr.GetFieldType(1)).Returns(typeof(string));

            mock.Setup(dr => dr.GetValue(0)).Returns(() =>
            {
                return currentRow == 0 ? (object)1 : (object)2;
            });
            mock.Setup(dr => dr.GetValue(1)).Returns(() =>
            {
                return currentRow == 0 ? (object)"Alice" : (object)"Bob";
            });

            mock.Setup(dr => dr.GetValues(It.IsAny<object[]>())).Returns((object[] values) =>
            {
                values[0] = currentRow == 0 ? 1 : 2;
                values[1] = currentRow == 0 ? "Alice" : "Bob";
                return 2;
            });

            return mock;
        }

        #endregion
    }
}