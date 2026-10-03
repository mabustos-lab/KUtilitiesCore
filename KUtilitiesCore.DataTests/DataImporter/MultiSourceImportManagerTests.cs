using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.DataTests.DataImporter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Tests
{
    [TestClass()]
    public class MultiSourceImportManagerTests
    {
        private TestContext? testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance!; }
            set { testContextInstance = value; }
        }

        [TestInitialize]
        public void InitializeFiles()
        {
            MultiSourceImportTestData.RootPath = TestContext.TestRunDirectory!;
            MultiSourceImportTestData.CreateFiles();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MultiSourceImportTestData.ClearFiles();
        }

        [TestMethod(DisplayName = "Operacion sin fuentes registradas lanza error de precondicion")]
        public void Import_WithoutSources_ThrowsPreconditionError()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());

            Assert.ThrowsExactly<InvalidOperationException>(() => manager.Import());
        }

        [TestMethod(DisplayName = "AddSource rechaza lector nulo")]
        public void AddSource_NullReader_ThrowsArgumentNull()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());

            Assert.ThrowsExactly<ArgumentNullException>(
                () => manager.AddSource(null!, "FuenteCSV"));
        }

        [TestMethod(DisplayName = "AddSource rechaza nombre de fuente nulo o vacio")]
        public void AddSource_NullOrEmptySourceName_ThrowsArgument()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            var reader = new CsvSourceReader(MultiSourceImportTestData.ValidDataPath);

            Assert.ThrowsExactly<ArgumentException>(
                () => manager.AddSource(reader, null!));
            Assert.ThrowsExactly<ArgumentException>(
                () => manager.AddSource(reader, string.Empty));
            Assert.ThrowsExactly<ArgumentException>(
                () => manager.AddSource(reader, "   "));
        }

        [TestMethod(DisplayName = "AddSource rechaza nombres de fuente duplicados")]
        public void AddSource_DuplicateSourceName_ThrowsArgument()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            var reader = new CsvSourceReader(MultiSourceImportTestData.ValidDataPath);
            manager.AddSource(reader, "FuenteCSV");

            Assert.ThrowsExactly<ArgumentException>(
                () => manager.AddSource(reader, "FuenteCSV"));
        }

        [TestMethod(DisplayName = "Constructor rechaza esquema nulo")]
        public void Ctor_NullFieldDefinitions_ThrowsArgumentNull()
        {
            Assert.ThrowsExactly<ArgumentNullException>(
                () => new MultiSourceImportManager(null!));
        }

        [TestMethod(DisplayName = "Import procesa N fuentes del mismo esquema en una sola operacion")]
        public void Import_MultipleSourcesSameSchema_ProcessesAllSources()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ReorderedDataPath), "FuenteReordenada");

            DataSet resultado = manager.Import();

            Assert.AreEqual(2, resultado.Tables.Count);
            Assert.AreEqual(4, resultado.Tables["FuenteCSV"]!.Rows.Count);
            Assert.AreEqual(2, resultado.Tables["FuenteReordenada"]!.Rows.Count);
            Assert.IsTrue(resultado.Tables["FuenteCSV"]!.Rows
                .Cast<DataRow>()
                .All(row => (bool)row["_IsValid"]));
        }

        [TestMethod(DisplayName = "Cada DataTable del resultado lleva el nombre de su fuente")]
        public void Import_ResultTables_NamedAfterSource()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ReorderedDataPath), "FuenteReordenada");

            DataSet resultado = manager.Import();

            CollectionAssert.AreEquivalent(
                new[] { "FuenteCSV", "FuenteReordenada" },
                resultado.Tables.Cast<DataTable>().Select(t => t.TableName).ToList());
        }

        [TestMethod(DisplayName = "Columnas reordenadas en la fuente se mapean al mismo esquema compartido")]
        public void Import_ReorderedColumns_MapToSameSchemaFields()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ReorderedDataPath), "FuenteReordenada");

            DataSet resultado = manager.Import();

            var fila = resultado.Tables["FuenteReordenada"]!.Rows[0];
            Assert.AreEqual("Pedro", fila["Name"]);
            Assert.AreEqual("Ruiz", fila["SecondName"]);
            Assert.AreEqual("22", fila["Edad"]);
            Assert.AreEqual("Bilbao", fila["City"]);
            Assert.AreEqual("Disenador", fila["Profession"]);
        }

        [TestMethod(DisplayName = "Los indices de fila del resultado son relativos a cada fuente")]
        public void Import_RowIndex_IsRelativePerSource()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ReorderedDataPath), "FuenteReordenada");

            DataSet resultado = manager.Import();

            var indicesFuenteCSV = resultado.Tables["FuenteCSV"]!.Rows
                .Cast<DataRow>()
                .Select(row => (int)row["_RowIndex"])
                .ToList();
            var indicesFuenteReordenada = resultado.Tables["FuenteReordenada"]!.Rows
                .Cast<DataRow>()
                .Select(row => (int)row["_RowIndex"])
                .ToList();

            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3 }, indicesFuenteCSV);
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, indicesFuenteReordenada);
        }

        [TestMethod(DisplayName = "Fallo de lectura en una fuente lanza MultiSourceImportException con la fuente y la causa")]
        public void Import_ReadFailureInOneSource_ThrowsMultiSourceExceptionWithSourceAndCause()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteValida");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.MissingDataPath), "FuenteInexistente");

            var exception = Assert.ThrowsExactly<MultiSourceImportException>(() => manager.Import());

            Assert.AreEqual(1, exception.Failures.Count);
            Assert.AreEqual("FuenteInexistente", exception.Failures[0].SourceName);
            Assert.IsNotNull(exception.Failures[0].ReadException);
            Assert.IsInstanceOfType(exception.Failures[0].ReadException, typeof(DataLoadException));
            var causaRaiz = exception.Failures[0].ReadException;
            while (causaRaiz!.InnerException != null)
            {
                causaRaiz = causaRaiz.InnerException;
            }

            Assert.IsInstanceOfType(causaRaiz, typeof(FileNotFoundException));
        }

        [TestMethod(DisplayName = "Fallo de lectura en varias fuentes reporta todas las fuentes fallidas")]
        public void Import_ReadFailureInSeveralSources_ReportsAllFailedSources()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.MissingDataPath), "FuenteA");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteValida");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.MissingDataPath), "FuenteB");

            var exception = Assert.ThrowsExactly<MultiSourceImportException>(() => manager.Import());

            CollectionAssert.AreEquivalent(
                new[] { "FuenteA", "FuenteB" },
                exception.Failures.Select(f => f.SourceName).ToList());
        }

        [TestMethod(DisplayName = "Error de validacion en una fuente reporta el fallo con indice relativo")]
        public void Import_ValidationErrorInOneSource_ReportsFailureBySourceWithRelativeIndex()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteValida");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.InvalidEdadDataPath), "FuenteEdadInvalida");

            var exception = Assert.ThrowsExactly<MultiSourceImportException>(() => manager.Import());

            Assert.AreEqual(1, exception.Failures.Count);
            var fallo = exception.Failures[0];
            Assert.AreEqual("FuenteEdadInvalida", fallo.SourceName);
            Assert.IsNull(fallo.ReadException);
            Assert.IsTrue(fallo.ValidationErrors.Any(e => e.IndexRow == 1));
        }

        [TestMethod(DisplayName = "Tras un fallo el estado del gestor permite reintentar la operacion")]
        public void Import_AfterFailure_StateAllowsRetryWithSameOutcome()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.InvalidEdadDataPath), "FuenteEdadInvalida");

            var primera = Assert.ThrowsExactly<MultiSourceImportException>(() => manager.Import());
            var segunda = Assert.ThrowsExactly<MultiSourceImportException>(() => manager.Import());

            Assert.AreEqual(primera.Failures.Count, segunda.Failures.Count);
            Assert.AreEqual("FuenteEdadInvalida", segunda.Failures[0].SourceName);
        }

        [TestMethod(DisplayName = "ImportAsync procesa N fuentes via ReadDataAsync en una sola operacion")]
        public async Task ImportAsync_MultipleSourcesSameSchema_ProcessesAllSources()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ReorderedDataPath), "FuenteReordenada");

            DataSet resultado = await manager.ImportAsync(CancellationToken.None);

            Assert.AreEqual(2, resultado.Tables.Count);
            Assert.AreEqual(4, resultado.Tables["FuenteCSV"]!.Rows.Count);
            Assert.AreEqual(2, resultado.Tables["FuenteReordenada"]!.Rows.Count);
        }

        [TestMethod(DisplayName = "ImportAsync con token ya cancelado aborta con OperationCanceledException sin procesar")]
        public async Task ImportAsync_AlreadyCancelledToken_ThrowsOperationCanceled()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsExactlyAsync<OperationCanceledException>(
                () => manager.ImportAsync(cts.Token));

            DataSet resultado = manager.Import();
            Assert.AreEqual(1, resultado.Tables.Count);
        }

        [TestMethod(DisplayName = "Cancelacion entre fuentes aborta sin resultados parciales y deja el estado intacto")]
        public async Task ImportAsync_CancellationBetweenSources_ThrowsAndLeavesStateUnchanged()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            using var cts = new CancellationTokenSource();
            manager.AddSource(
                new CancelTriggerSourceReader(() => cts.Cancel()), "FuenteCancelada");
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");

            await Assert.ThrowsExactlyAsync<OperationCanceledException>(
                () => manager.ImportAsync(cts.Token));

            DataSet reintento = manager.Import();
            Assert.AreEqual(2, reintento.Tables.Count);
            Assert.AreEqual(1, reintento.Tables["FuenteCancelada"]!.Rows.Count);
            Assert.AreEqual(4, reintento.Tables["FuenteCSV"]!.Rows.Count);
        }

        [TestMethod(DisplayName = "Fallo de lectura en ImportAsync reporta la fuente con la causa envuelta")]
        public async Task ImportAsync_ReadFailure_ThrowsMultiSourceExceptionWithSourceAndCause()
        {
            using var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.MissingDataPath), "FuenteInexistente");

            var exception = await Assert.ThrowsExactlyAsync<MultiSourceImportException>(
                () => manager.ImportAsync(CancellationToken.None));

            Assert.AreEqual(1, exception.Failures.Count);
            Assert.AreEqual("FuenteInexistente", exception.Failures[0].SourceName);
            Assert.IsInstanceOfType(exception.Failures[0].ReadException, typeof(DataLoadException));
        }

        [TestMethod(DisplayName = "AddSource tras Dispose lanza ObjectDisposedException")]
        public void AddSource_AfterDispose_ThrowsObjectDisposed()
        {
            var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            manager.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(
                () => manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteNueva"));
        }

        [TestMethod(DisplayName = "Import tras Dispose lanza ObjectDisposedException")]
        public void Import_AfterDispose_ThrowsObjectDisposed()
        {
            var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            manager.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(() => manager.Import());
        }

        [TestMethod(DisplayName = "ImportAsync tras Dispose lanza ObjectDisposedException")]
        public async Task ImportAsync_AfterDispose_ThrowsObjectDisposed()
        {
            var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            manager.Dispose();

            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(
                () => manager.ImportAsync(CancellationToken.None));
        }

        [TestMethod(DisplayName = "Dispose es idempotente y no lanza al llamarse dos veces")]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var manager = new MultiSourceImportManager(GetBasicMapping());
            manager.AddSource(new CsvSourceReader(MultiSourceImportTestData.ValidDataPath), "FuenteCSV");
            manager.Dispose();

            manager.Dispose();
        }

        private static FieldDefinitionCollection GetBasicMapping()
        {
            var result = new FieldDefinitionCollection();
            result.AddRange(
                new FieldDefinitionItem[]
                {
                    new FieldDefinitionItem("Name", "Nombre"),
                    new FieldDefinitionItem("SecondName", "Apellido"),
                    new FieldDefinitionItem("Edad", "Edad", fieldType: typeof(int)),
                    new FieldDefinitionItem("City", "Ciudad"),
                    new FieldDefinitionItem("Profession", "Profesion")
                });
            return result;
        }

        /// <summary>
        /// Lector de pruebas que devuelve una fila válida y dispara una acción
        /// (usada para cancelar el token) al leerse de forma asíncrona.
        /// </summary>
        private sealed class CancelTriggerSourceReader : IDataSourceReader
        {
            private readonly Action _onReadAsync;

            public CancelTriggerSourceReader(Action onReadAsync)
            {
                _onReadAsync = onReadAsync;
            }

            public bool CanRead => true;

            public DataTable ReadData() => BuildData();

            public Task<DataTable> ReadDataAsync()
            {
                _onReadAsync();
                return Task.FromResult(BuildData());
            }

            private static DataTable BuildData()
            {
                var table = new DataTable();
                table.Columns.Add("Nombre");
                table.Columns.Add("Apellido");
                table.Columns.Add("Edad");
                table.Columns.Add("Ciudad");
                table.Columns.Add("Profesion");
                table.Rows.Add("Laura", "Sanchez", "40", "Cuenca", "Arquitecto");
                return table;
            }
        }
    }
}
