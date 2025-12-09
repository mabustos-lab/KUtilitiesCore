using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.Data.DataImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KUtilitiesCore.Data.ImportDefinition;
using System.Diagnostics;
using KUtilitiesCore.Extensions;

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
                if (manager.ValidationErrors.ErrorMessages.Any())
                {
                    Debug.WriteLine("--Mensajes--");
                    Debug.WriteLine(string.Join("\n", manager.ValidationErrors.ErrorMessages));
                    Debug.WriteLine("--Celdas no válidas--");
                    Debug.WriteLine(string.Join("\n", manager.ValidationErrors.Errors));                    
                }

            }
            Debug.WriteLine("--Datos importados--");

            manager.Data.PrintPretty(true);
            return isValid;
        }

        private static FielDefinitionCollection GetBasicMapping()
        {
            FielDefinitionCollection result = new FielDefinitionCollection();
            result.AddRange(
                new FieldDefinition[]
                {
                    new FieldDefinition("Name", "Nombre"),
                    new FieldDefinition("SecondName", "Apellido"),
                    new FieldDefinition("Edad", "Edad", fieldType: typeof(int)),
                    new FieldDefinition("City", "Ciudad"),
                    new FieldDefinition("Profession", "Profesion")
                });
            return result;
        }
    }
}