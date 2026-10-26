using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.DataTests.DataImporter;
using KUtilitiesCore.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Tests
{
    [TestClass()]
    public class ImportManagerTests
    {
        private TestContext testContextInstance;

        public TestContext TestContext { get { return testContextInstance; } set { testContextInstance = value; } }

        [TestInitialize]
        public void InitializeFiles()
        {
            CsvSourceReaderDatagenerator.RootPath = TestContext.TestRunDirectory;
            CsvSourceReaderDatagenerator.CreateDataTest();
        }

        [TestCleanup]
        public void Cleanup() { CsvSourceReaderDatagenerator.ClearFilesTest(); }
        [TestMethod("ImportManager Basic Load Test")]
        public void ImportManagerBasicLoadTest()
        {
            var manager = new ImportManager();
            manager.SetMapping(GetBasicMapping());
            var csvReader = new CsvSourceReader(CsvSourceReaderDatagenerator.BasicDataPath);
            manager.LoadData(csvReader);
            Assert.IsTrue(ValidateImport(manager));
        }
        [TestMethod("ImportManager Basic Load Test Excel")]
        public void ImportManagerBasicLoadExcelTest()
        {
            var manager = new ImportManager();
            manager.SetMapping(GetBasicMapping());
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDir, "TestData", "datos_test.xlsx");
            using (var xlsxReader = ExcelSourceReaderFactory.Create(filePath, "TestData"))
            {
                xlsxReader.SheetName = "datos_basicos";
                manager.LoadData(xlsxReader);
                Assert.IsTrue(ValidateImport(manager));
            }            
        }
        [TestMethod("ImportManager Empty Load IsValid Test")]
        public void ImportManager_EmptyLoad_IsValid_Test()
        {
            var manager = new ImportManager();
            manager.SetMapping(GetBasicMapping());
            var csvReader = new CsvSourceReader(CsvSourceReaderDatagenerator.EmptyDataPath);
            manager.LoadData(csvReader);
            Assert.IsFalse(ValidateImport(manager));
        }
        [TestMethod("ImportManager Diferent Mapping Load Test")]
        public void ImportManagerMappingLoadTest()
        {
            var manager = new ImportManager();
            var mapping = GetBasicMapping();
            manager.SetMapping(GetBasicMapping());
            // Cargamos una columna requerida Apellido, fuente Apellidos
            var csvReader = new CsvSourceReader(CsvSourceReaderDatagenerator.MappingDataPath);
            manager.LoadData(csvReader);
            if (!ValidateImport(manager))
            {
                //Emulamos correcion de mapeo
                manager.ColumnDefinitions["SecondName"].SourceColumnName = "Apellidos";
                manager.LoadData(csvReader);
            }
            Assert.IsTrue(ValidateImport(manager));
        }

        [TestMethod("ImportManager ReadData Paints Column Errors On Provided Table")]
        public void ReadData_PaintsColumnErrors_OnProvidedTable()
        {
            var raw = new DataTable();
            raw.Columns.Add("Nombre", typeof(string));
            raw.Columns.Add("Edad", typeof(string));
            raw.Rows.Add("Juan Perez", "30");
            raw.Rows.Add("Ana Gomez", "25x");

            var manager = new ImportManager();
            manager.SetMapping(GetSimpleMapping());
            manager.ReadData(raw);
            manager.ValidateDataTypes();

            Assert.IsFalse(raw.Rows[0].HasErrors, "La fila valida no debe quedar con errores.");
            Assert.IsTrue(raw.Rows[1].HasErrors,
                "La fila con valor no convertible debe quedar marcada con error en la tabla provista por el llamador.");
        }

        [TestMethod("ImportManager ValidateDataTypes Applies DefaultValue On Provided Table")]
        public void ValidateDataTypes_AppliesDefaultValue_OnProvidedTable()
        {
            var raw = new DataTable();
            raw.Columns.Add("Nombre", typeof(string));
            raw.Columns.Add("Edad", typeof(string));
            raw.Rows.Add("Juan Perez", string.Empty);

            var manager = new ImportManager();
            var mapping = GetSimpleMapping();
            mapping["Age"].DefaultValue = 0;
            manager.SetMapping(mapping);
            manager.ReadData(raw);
            manager.ValidateDataTypes();

            Assert.AreEqual("0", raw.Rows[0]["Edad"],
                "El valor por defecto debe aplicarse sobre la tabla provista por el llamador.");
        }

        [TestMethod("ImportManager ValidateDataTypes Empty Table Returns False")]
        public void ValidateDataTypes_EmptyTable_ReturnsFalse()
        {
            var raw = new DataTable();
            raw.Columns.Add("Nombre", typeof(string));
            raw.Columns.Add("Edad", typeof(string));

            var manager = new ImportManager();
            manager.SetMapping(GetSimpleMapping());
            manager.ReadData(raw);

            bool result = manager.ValidateDataTypes();

            Assert.IsFalse(result,
                "Una tabla sin filas no es un conjunto de datos importable: el retorno debe ser false.");
            Assert.IsFalse(manager.ValidationErrors.IsValid,
                "El estado de errores debe ser coherente con el retorno: con cero filas IsValid debe ser false.");
        }

        [TestMethod("ImportManager CreateResultTable Returns Typed Table Without Control Columns")]
        public void CreateResultTable_ReturnsTypedTable_WithoutControlColumns()
        {
            var raw = new DataTable();
            raw.Columns.Add("Nombre", typeof(string));
            raw.Columns.Add("Edad", typeof(string));
            raw.Rows.Add("Juan Perez", "30");
            raw.Rows.Add("Ana Gomez", string.Empty);
            raw.Rows.Add(DBNull.Value, "40");

            var manager = new ImportManager();
            var mapping = GetSimpleMapping();
            mapping["Age"].DefaultValue = 0;
            manager.SetMapping(mapping);
            manager.ReadData(raw);

            var result = manager.CreateResultTable();

            Assert.IsFalse(result.Columns.Contains("_RowIndex"),
                "La tabla resultado no debe exponer columnas de control del proceso.");
            Assert.IsFalse(result.Columns.Contains("_IsValid"),
                "La tabla resultado no debe exponer columnas de control del proceso.");
            var ageColumn = result.Columns["Age"];
            Assert.IsNotNull(ageColumn, "La tabla resultado debe incluir la columna Age.");
            Assert.AreEqual(typeof(int), ageColumn.DataType,
                "La columna Age debe quedar tipada con el tipo destino de su definicion.");
            Assert.AreEqual(30, result.Rows[0]["Age"],
                "El valor string debe convertirse al tipo destino.");
            Assert.AreEqual(0, result.Rows[1]["Age"],
                "La celda vacia con DefaultValue debe resolverse al valor por defecto.");
            Assert.AreEqual(DBNull.Value, result.Rows[2]["Name"],
                "La celda vacia sin DefaultValue debe quedar en DBNull.");
        }

        private bool ValidateImport(ImportManager manager)
        {
            bool isValid = manager.ValidateDataTypes();
            if (!isValid)
            {
                Debug.WriteLine("--Datos no válidos--");
                if (manager.ValidationErrors.Errors.Any())
                {
                    Debug.WriteLine("--Mensajes--");
                    Debug.WriteLine(string.Join("\n", manager
                        .ValidationErrors.Errors.Select(x => x.ToString())));
                    
                }

            }
            Debug.WriteLine("--Datos importados--");

            manager.DataSource.PrintPretty(true);
            return isValid;
        }

        private static FieldDefinitionCollection GetSimpleMapping()
        {
            FieldDefinitionCollection result = new FieldDefinitionCollection();
            result.AddRange(
                new FieldDefinitionItem[]
                {
                    new FieldDefinitionItem("Name", "Nombre"),
                    new FieldDefinitionItem("Age", "Edad", fieldType: typeof(int))
                });
            return result;
        }

        private static FieldDefinitionCollection GetBasicMapping()
        {
            FieldDefinitionCollection result = new FieldDefinitionCollection();
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
    }
}