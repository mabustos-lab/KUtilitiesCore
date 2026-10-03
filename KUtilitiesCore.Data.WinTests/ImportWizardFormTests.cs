using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Win.Importer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;

using System.Windows.Forms;

namespace KUtilitiesCore.Data.WinTests
{
    [TestClass]
    public sealed class ImportWizardFormTests
    {
        public class TestableImportWizardForm : ImportWizardForm
        {
            private FieldDefinitionCollection _testFields;

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public DataTable? MockDataTableToReturn { get; set; }
            public string? LastMessageShown { get; private set; }
            public MessageBoxIcon LastMessageIcon { get; private set; }

            /// <summary>Simula un fallo crítico dentro del pipeline de importación para probar el manejo de errores.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool ThrowOnProcessFinished { get; set; }

            public TestableImportWizardForm(FieldDefinitionCollection fields)
                : base(fields, new ImportManager()) // Pasamos un ImportManager real o mock
            {
                _testFields = fields.Clone();
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

            private DataTable GetSampleWithErrorValueDataTable()
            {
                var dt = new DataTable();
                dt.Columns.Add("Nombre", typeof(string));
                dt.Columns.Add("Edad", typeof(string)); // En CSV todo suele llegar como string al inicio
                dt.Rows.Add("Juan Perez", "30");
                dt.Rows.Add("Ana Gomez", "25x");
                return dt;
            }
            private DataTable GetSampleWithErrorColumnDataTable()
            {
                var dt = new DataTable();
                dt.Columns.Add("Nombre", typeof(string));
                dt.Columns.Add("Columna1", typeof(string)); // En CSV todo suele llegar como string al inicio
                dt.Rows.Add("Juan Perez", "30");
                dt.Rows.Add("Ana Gomez", "25");
                return dt;
            }
            public void SimulateWithErrorLoadData()
            {
                LoadedDataTable = GetSampleWithErrorValueDataTable();
            }
            public void SimulateWithErrorColumnNameLoadData()
            {
                LoadedDataTable = GetSampleWithErrorColumnDataTable();
            }
            public DataGridView GetGridPreview => this.dgvPreview;
            public DataGridView GetGridMapping => this.dgvMapping;
            public DataGridView GetGridErrors => this.dgvErrors;
            public Button GetImportButton => this.btnImport;
            public Button GetCancelButton => this.btnCancel;
            public ToolStripStatusLabel GetWarningLabel => this.tsslWarning;
            public override void LoadData()
            {
                LoadedDataTable = GetSampleDataTable();
            }
            public void SimulateCorrectingMapping()
            {
                dgvMapping.Rows[1].Cells[1].Value = "Columna1";
            }
            public void SimulateImport()
            {
                ImportData();
            }
            /// <summary>Invoca la revalidación silenciosa que dispara la edición de una celda del preview.</summary>
            public void SimulateCellEditRevalidation()
            {
                RevalidateAfterEdit();
            }
            public void ClearLastMessage()
            {
                LastMessageShown = null;
            }
            protected override void OnProcessImportFinished()
            {
                if (ThrowOnProcessFinished)
                    throw new InvalidOperationException("Fallo crítico simulado en el proceso de importación.");
            }
            // Capturamos mensajes para asserts en lugar de mostrarlos
            protected override void ShowMessage(string message, string caption, MessageBoxIcon msgIcon)
            {
                LastMessageShown = message;
                LastMessageIcon = msgIcon;
                Console.WriteLine($"[UI Message]: {message}");
            }
        }

        private FieldDefinitionCollection GetSampleDefinitions(bool addRule = false)
        {
            var defs = new FieldDefinitionCollection();
            defs.Add(new FieldDefinitionItem("Name", "Nombre"));
            defs.Add(new FieldDefinitionItem("Age", "Edad", fieldType: typeof(int)));
            if (addRule)
                defs["Age"].WithRules(
                    rules =>
                    {
                        rules.LessThan(30, "El usuario debe tener más de 25 años");
                    });
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

                // Con autocierre el éxito NO debe mostrar MessageBox: el cierre es automático.
                Assert.IsNull(form.LastMessageShown,
                    "Con AutoCloseOnSuccess activo no debe mostrarse diálogo de éxito; el cierre debe ser automático.");
            }
        }
        [TestMethod]
        public void Import_ShouldShowSuccessMessage_WhenAutoCloseDisabled()
        {
            // Arrange
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.AutoCloseOnSuccess = false;
                form.ShowOpenDialogFile();
                form.LoadData();
                form.SimulateImport();

                // Sin autocierre el usuario necesita el mensaje informativo y el formulario permanece abierto.
                StringAssert.Contains("Importación completada y validada correctamente.", form.LastMessageShown);
                Assert.AreEqual(MessageBoxIcon.Information, form.LastMessageIcon);
                Assert.IsNotNull(form.ResultData, "ResultData no debería ser nulo.");
                Assert.AreNotEqual(DialogResult.OK, form.DialogResult, "Con AutoCloseOnSuccess=false el formulario no debe cerrarse.");
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
                Assert.IsNotNull(form.LastMessageShown);
                StringAssert.Contains(form.LastMessageShown, "errores de validación");
                Assert.AreEqual(MessageBoxIcon.Warning, form.LastMessageIcon);
            }
        }
        [TestMethod]
        public void Import_ShouldShowErrorsColumName_WhenDataIsInvalid()
        {
            // Arrange
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                // 1. Simular selección de archivo
                form.ShowOpenDialogFile();

                // 2. Simular click en Cargar
                form.SimulateWithErrorColumnNameLoadData();
                form.SimulateImport();

                // Assert
                Assert.AreNotEqual(DialogResult.OK, form.DialogResult, "El formulario NO debería cerrarse si hay errores.");
                Assert.IsNull(form.ResultData, "ResultData debería ser nulo.");
                Assert.IsNotNull(form.LastMessageShown);
                StringAssert.Contains(form.LastMessageShown, "errores de validación");
                Assert.AreEqual(MessageBoxIcon.Warning, form.LastMessageIcon);
            }
        }
        [TestMethod]
        public void Import_ShouldSucceed_WhenCorrectMapping()
        {
            // Arrange
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                // 1. Simular selección de archivo
                form.ShowOpenDialogFile();

                // 2. Simular click en Cargar
                form.SimulateWithErrorColumnNameLoadData();
                form.SimulateCorrectingMapping();
                form.SimulateImport();
                Assert.AreEqual(DialogResult.OK, form.DialogResult, "El formulario debería cerrarse con OK si la importación es exitosa.");
                Assert.IsNotNull(form.ResultData, "ResultData no debería ser nulo.");
                Assert.HasCount(2, form.ResultData.Rows, "Deberían haberse importado 2 objetos.");

                // Con autocierre el éxito NO debe mostrar MessageBox: el cierre es automático.
                Assert.IsNull(form.LastMessageShown,
                    "Con AutoCloseOnSuccess activo no debe mostrarse diálogo de éxito; el cierre debe ser automático.");
            }
        }
        [TestMethod]
        public void Import_ShouldShowErrors_WhenDataRuleIsInValid()
        {
            // Arrange
            var defs = GetSampleDefinitions(true);
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                // 1. Simular selección de archivo
                form.ShowOpenDialogFile();

                // 2. Simular click en Cargar
                form.LoadData();
                form.SimulateImport();
                Assert.AreNotEqual(DialogResult.OK, form.DialogResult, "El formulario NO debería cerrarse si hay errores.");
                Assert.IsNull(form.ResultData, "ResultData debería ser nulo.");
                Assert.IsNotNull(form.LastMessageShown);
                StringAssert.Contains(form.LastMessageShown, "errores de validación");
                Assert.AreEqual(MessageBoxIcon.Warning, form.LastMessageIcon);
            }
        }
        [STATestMethod]
        public void Import_CriticalError_ShouldKeepPersistentWarningInUi()
        {
            // Arrange: un fallo crítico en el pipeline no debe pasar desapercibido.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ThrowOnProcessFinished = true;
                form.ShowOpenDialogFile();
                form.LoadData();
                form.Show(); // La visibilidad de un ToolStripItem depende de su contenedor.
                try
                {
                    form.SimulateImport();

                    Assert.IsNotNull(form.LastMessageShown);
                    StringAssert.Contains(form.LastMessageShown, "Error crítico");
                    Assert.IsTrue(form.GetWarningLabel.Visible,
                        "El error crítico debe quedar visible persistentemente en la barra de estado, no solo en un MessageBox transitorio.");
                    Assert.AreNotEqual(DialogResult.OK, form.DialogResult, "Un error crítico no debe cerrar el formulario con OK.");
                    Assert.IsNull(form.ResultData, "Con error crítico no debe exponerse resultado.");
                }
                finally
                {
                    if (!form.IsDisposed) form.Close();
                }
            }
        }
        [STATestMethod]
        public void BtnImport_PerformClick_InvalidData_ShouldNotCloseWithOk()
        {
            // Guardia de regresión del camino real del clic: en WinForms el botón
            // asigna su DialogResult al formulario ANTES del manejador Click, por lo
            // que el código es quien decide el cierre final (None con errores).
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();
                form.SimulateWithErrorLoadData();
                form.Show();
                try
                {
                    // Act: clic real sobre el botón Importar.
                    form.GetImportButton.PerformClick();

                    // Assert: el formulario debe permanecer abierto para que el usuario corrija.
                    Assert.AreNotEqual(DialogResult.OK, form.DialogResult,
                        "El clic en Importar NO debe cerrar con OK cuando hay errores de validación.");
                    Assert.IsNull(form.ResultData, "Con errores de validación no debe exponerse resultado.");
                    Assert.IsNotNull(form.LastMessageShown);
                    StringAssert.Contains(form.LastMessageShown, "errores de validación");
                    Assert.AreNotEqual(0, form.GetGridErrors.Rows.Count,
                        "El grid de errores debería listar los fallos de validación.");
                }
                finally
                {
                    if (!form.IsDisposed) form.Close();
                }
            }
        }
        [STATestMethod]
        public void BtnImport_PerformClick_ValidData_ShouldCloseWithOk()
        {
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();
                form.LoadData();
                form.Show();
                try
                {
                    // Act: clic real sobre el botón Importar.
                    form.GetImportButton.PerformClick();

                    // Assert: éxito → autocierre con OK y resultado expuesto.
                    Assert.AreEqual(DialogResult.OK, form.DialogResult,
                        "El clic en Importar con datos válidos debería cerrar el formulario con OK.");
                    Assert.IsNotNull(form.ResultData, "ResultData no debería ser nulo.");
                    Assert.HasCount(2, form.ResultData.Rows, "Deberían haberse importado 2 objetos.");
                }
                finally
                {
                    if (!form.IsDisposed) form.Close();
                }
            }
        }
        [STATestMethod]
        public void BtnCancel_PerformClick_ShouldReturnNativeCancel()
        {
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.Show();
                try
                {
                    // Act: clic real sobre el botón Cancelar.
                    form.GetCancelButton.PerformClick();

                    // Assert: resultado equivalente a la cancelación nativa de Windows.
                    Assert.AreEqual(DialogResult.Cancel, form.DialogResult,
                        "Cancelar debe retornar DialogResult.Cancel (cancelación nativa de Windows).");
                }
                finally
                {
                    if (!form.IsDisposed) form.Close();
                }
            }
        }
        [STATestMethod]
        public void BtnImport_ModalDialog_InvalidData_ShouldNotCloseWithOk()
        {
            // Escenario modal real (ShowDialog en un hilo STA dedicado): es el único
            // camino que reproduce la interacción nativa del usuario con el asistente,
            // donde el botón Importar tiene DialogResult asignado desde el Designer.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                // Sin forzar el Handle: el identificador lo creará el hilo del diálogo.
                form.ShowOpenDialogFile();
                form.SimulateWithErrorLoadData();

                var dialogResult = DialogResult.None;
                var dialogThread = new Thread(() => dialogResult = form.ShowDialog());
                dialogThread.SetApartmentState(ApartmentState.STA);
                dialogThread.Start();

                var wait = TimeSpan.FromSeconds(10);
                try
                {
                    // Esperar a que el diálogo modal sea visible.
                    var sw = Stopwatch.StartNew();
                    while (!form.Visible && sw.Elapsed < wait)
                        Thread.Sleep(50);
                    Assert.IsTrue(form.Visible, "El diálogo modal debería estar visible.");

                    // Clic real sobre Importar, ejecutado en el hilo del diálogo.
                    form.BeginInvoke(new Action(() => form.GetImportButton.PerformClick()));

                    // Esperar a que el clic se procese (mensaje de validación registrado).
                    sw.Restart();
                    while (form.LastMessageShown is null && sw.Elapsed < wait && dialogThread.IsAlive)
                        Thread.Sleep(50);

                    if (dialogThread.IsAlive)
                    {
                        // El diálogo permanece abierto (comportamiento esperado):
                        // cerrarlo debe retornar la cancelación nativa de Windows.
                        form.BeginInvoke(new Action(() => form.Close()));
                        Assert.IsTrue(dialogThread.Join(wait), "El diálogo debería cerrarse tras Close().");
                        Assert.AreEqual(DialogResult.Cancel, dialogResult,
                            "Cerrar manualmente un diálogo sin resultado debe retornar DialogResult.Cancel.");
                    }
                    else
                    {
                        // El clic cerró el diálogo: con errores de validación NO debe haber sido con OK.
                        Assert.AreNotEqual(DialogResult.OK, dialogResult,
                            "BUG: el clic en Importar cerró el diálogo modal con OK aun con errores de validación.");
                    }

                    Assert.IsNull(form.ResultData, "Con errores de validación no debe exponerse resultado.");
                }
                finally
                {
                    if (dialogThread.IsAlive)
                    {
                        form.BeginInvoke(new Action(() => form.Close()));
                        dialogThread.Join(wait);
                    }
                }
            }
        }
        [STATestMethod]
        public void CellEdit_Revalidation_ShouldBeSilent_AndRefreshUi()
        {
            // La edición del preview revalida en silencio: sin diálogos ni cierre,
            // manteniendo los errores visibles hasta que el usuario los corrige.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();
                form.SimulateWithErrorLoadData();
                form.Show(); // La visibilidad de un ToolStripItem depende de su contenedor.
                try
                {
                    // Act 1: el usuario edita una celda con errores aún presentes.
                    form.ClearLastMessage();
                    form.SimulateCellEditRevalidation();

                    Assert.IsNull(form.LastMessageShown, "La revalidación por edición debe ser silenciosa.");
                    Assert.AreNotEqual(DialogResult.OK, form.DialogResult, "La edición no debe cerrar el formulario.");
                    Assert.IsNull(form.ResultData, "Con errores persistentes no debe exponerse resultado.");
                    Assert.IsTrue(form.GetWarningLabel.Visible, "El aviso de errores debe permanecer visible en la barra de estado.");
                    Assert.AreNotEqual(0, form.GetGridErrors.Rows.Count, "El grid de errores debe listar los fallos detectados.");

                    // Act 2: el usuario corrige el valor subyacente y vuelve a editarse.
                    form.LoadedDataTable!.Rows[1]["Edad"] = "25";
                    form.SimulateCellEditRevalidation();

                    Assert.IsNull(form.LastMessageShown, "La corrección exitosa tampoco debe mostrar diálogos.");
                    Assert.IsFalse(form.GetWarningLabel.Visible, "Sin errores el aviso debe ocultarse.");
                    Assert.AreEqual(0, form.GetGridErrors.Rows.Count, "El grid de errores debe vaciarse tras corregir.");
                    Assert.IsNotNull(form.ResultData, "Con datos válidos el resultado debe quedar expuesto.");
                    Assert.AreNotEqual(DialogResult.OK, form.DialogResult, "La edición correctiva no debe cerrar el formulario.");
                }
                finally
                {
                    if (!form.IsDisposed) form.Close();
                }
            }
        }
        [TestMethod]
        public void Form_ShouldWireCancelButtonForEsc()
        {
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                Assert.AreSame(form.GetCancelButton, form.CancelButton,
                    "Form.CancelButton debe apuntar al botón Cancelar para que ESC cancele (comportamiento nativo de Windows).");
            }
        }
    }
}
