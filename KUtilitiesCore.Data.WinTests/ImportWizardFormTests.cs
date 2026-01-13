using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Win.Importer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

using System.Windows.Forms;
using FieldDefinition = KUtilitiesCore.Data.ImportDefinition.FieldDefinitionItem;

namespace KUtilitiesCore.Data.WinTests
{
    [TestClass]
    public sealed class ImportWizardFormTests
    {
        public class TestableImportWizardForm : ImportWizardForm
        {
            public DataTable MockDataTableToReturn { get; set; }
            public string LastMessageShown { get; private set; }
            public MessageBoxIcon LastMessageIcon { get; private set; }

            public TestableImportWizardForm(FieldDefinitionCollection fields)
                : base(fields, new ImportManager()) // Pasamos un ImportManager real o mock
            {
            }
            public override void ShowOpenDialogFile()
            {
                FileName = "C:\\fake\\path\\test_data.csv";
            }

            private DataTable GetSampleDataTable()
            {
                var dt = new DataTable();
                dt.Columns.Add("Nombre", typeof(string));
                dt.Columns.Add("Edad", typeof(string)); // En CSV todo suele llegar como string al inicio
                dt.Rows.Add("Juan Perez", "30");
                dt.Rows.Add("Ana Gomez", "25");
                return dt;
            }

            private DataTable GetSampleWithErrorDataTable()
            {
                var dt = new DataTable();
                dt.Columns.Add("Nombre", typeof(string));
                dt.Columns.Add("Edad", typeof(string)); // En CSV todo suele llegar como string al inicio
                dt.Rows.Add("Juan Perez", "30");
                dt.Rows.Add("Ana Gomez", "25x");
                return dt;
            }
            public void SimulateWithErrorLoadData()
            {
                LoadedDataTable = GetSampleWithErrorDataTable();
            }
            public DataGridView GetGridPreview => this.dgvPreview;
            public DataGridView GetGridMapping => this.dgvMapping;
            public override void LoadData()
            {
                LoadedDataTable = GetSampleDataTable();
            }
            public void SimulateImport()
            {
                ImportData();
            }
            protected override void OnProcessImportFinished()
            {

            }
            // Capturamos mensajes para asserts en lugar de mostrarlos
            protected override void ShowMessage(string message, string caption, MessageBoxIcon msgIcon)
            {
                LastMessageShown = message;
                LastMessageIcon = msgIcon;
                Console.WriteLine($"[UI Message]: {message}");
            }
        }

        private FieldDefinitionCollection GetSampleDefinitions()
        {
            var defs = new FieldDefinitionCollection();
            defs.Add(new FieldDefinition("Name", "Nombre"));
            defs.Add(new FieldDefinition("Age", "Edad",fieldType:typeof(int)));
            return defs;
        }
        [TestMethod]
        public void LoadData_ShouldPopulateGrid_WhenFileIsSimulated()
        {
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                // Act
                // 1. Simular selección de archivo
                form.ShowOpenDialogFile();

                // 2. Simular click en Cargar
                form.LoadData();

                //form.ShowDialog();

                // Assert
                Assert.IsNotNull(form.LoadedDataTable, "El DataTable interno debería haberse llenado.");
                Assert.HasCount(2, form.LoadedDataTable.Rows, "Debería haber 2 filas cargadas.");
                Assert.HasCount(2, form.GetGridPreview.Rows, "El Grid de previsualización debería tener 2 filas.");

                // Verificar automapeo
                Assert.HasCount(2, form.GetGridMapping.Rows, "Debería haber 2 filas en el grid de mapeo.");
                Assert.AreEqual("Nombre", form.GetGridMapping.Rows[0].Cells[1].Value, "La columna 'Nombre' debería haberse mapeado automáticamente.");
            }
        }
        [TestMethod]
        public void Import_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                // 1. Simular selección de archivo
                form.ShowOpenDialogFile();

                // 2. Simular click en Cargar
                form.LoadData();
                form.SimulateImport();
                Assert.AreEqual(DialogResult.OK, form.DialogResult, "El formulario debería cerrarse con OK si la importación es exitosa.");
                Assert.IsNotNull(form.ResultData, "ResultData no debería ser nulo.");
                Assert.HasCount(2, form.ResultData.Rows, "Deberían haberse importado 2 objetos.");

                // Verificar mensaje de éxito
                StringAssert.Contains("Importación completada y validada correctamente.", form.LastMessageShown);
                Assert.AreEqual(MessageBoxIcon.Information, form.LastMessageIcon);
            }
        }
        [TestMethod]
        public void Import_ShouldShowErrors_WhenDataIsInvalid()
        {
            // Arrange
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                // 1. Simular selección de archivo
                form.ShowOpenDialogFile();

                // 2. Simular click en Cargar
                form.SimulateWithErrorLoadData();
                form.SimulateImport();
                // form.ShowDialog();
                // Assert
                Assert.AreNotEqual(DialogResult.OK, form.DialogResult, "El formulario NO debería cerrarse si hay errores.");
                Assert.IsNull(form.ResultData, "ResultData debería ser nulo.");
                StringAssert.Contains(form.LastMessageShown, "errores de validación");
                Assert.AreEqual(MessageBoxIcon.Warning, form.LastMessageIcon);
            }
        }
    }
}
