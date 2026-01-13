using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.DataTests.DataImporter;
using KUtilitiesCore.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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