using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
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

            /// <summary>
            /// Cuando está activo, <see cref="CreateReader"/> devuelve un lector de pruebas
            /// (stub) en lugar del lector real, sin tocar el sistema de archivos.
            /// </summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool UseStubReader { get; set; }

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
            /// <summary>Tabla de origen vacía: solo encabezados (Nombre/Edad), sin filas (p. ej. CSV de solo encabezados).</summary>
            public DataTable GetEmptyDataTable()
            {
                var dt = new DataTable();
                dt.Columns.Add("Nombre", typeof(string));
                dt.Columns.Add("Edad", typeof(string));
                return dt;
            }
            public void SimulateEmptyLoadData()
            {
                LoadedDataTable = GetEmptyDataTable();
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

            /// <summary>
            /// Sustituye el lector real por el stub de pruebas cuando <see cref="UseStubReader"/> está activo,
            /// permitiendo ejercitar la carga (incluida la asíncrona) sin depender de archivos físicos.
            /// </summary>
            protected override IDataSourceReader? CreateReader()
            {
                return UseStubReader ? new StubDataSourceReader(GetSampleDataTable) : base.CreateReader();
            }

            /// <summary>
            /// Lector de pruebas: completa la lectura tras una breve espera para reproducir
            /// una operación de E/S asíncrona que libera al llamador mientras el lector trabaja.
            /// </summary>
            private sealed class StubDataSourceReader : IDataSourceReader
            {
                private readonly Func<DataTable> _tableFactory;

                public StubDataSourceReader(Func<DataTable> tableFactory) => _tableFactory = tableFactory;

                public bool CanRead => true;

                public DataTable ReadData() => _tableFactory();

                public async Task<DataTable> ReadDataAsync()
                {
                    await Task.Delay(50).ConfigureAwait(false);
                    return _tableFactory();
                }
            }
            public void SimulateCorrectingMapping()
            {
                dgvMapping.Rows[1].Cells[1].Value = "Columna1";
            }
            /// <summary>Simula que el usuario cambia el mapeo de una columna origen en el grid de mapeo.</summary>
            public void SimulateMappingChange()
            {
                SimulateCorrectingMapping();
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
            /// <summary>Invoca el filtrado solo-errores del preview como lo hace el checkbox correspondiente.</summary>
            public void InvokeFilterHasErrors(bool showErrorOnly)
            {
                FilterHasErrors(LoadedDataTable!, dgvPreview, showErrorOnly);
            }
            /// <summary>Dispara el manejador de cambio de opciones de análisis como lo haría el control de configuración.</summary>
            public void SimulateOptionsChanged()
            {
                OnConfigOptionsChanged(null, EventArgs.Empty);
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
        [STATestMethod]
        public async Task LoadDataAsync_ShouldLoad_WithoutBlockingCaller()
        {
            // La carga asíncrona debe liberar al llamador mientras el lector trabaja:
            // el pipeline de UI (preview, mapeo, botones) se actualiza al completarse.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ClearLastMessage();
                form.ShowOpenDialogFile();
                form.UseStubReader = true;

                // Act
                var loadTask = form.LoadDataAsync();
                Assert.IsFalse(loadTask.IsCompleted,
                    "La carga asíncrona no debe bloquear al llamador: la tarea debe seguir pendiente mientras el lector lee.");

                // Bombeo de mensajes para que las continuaciones de UI (WinForms SynchronizationContext) se ejecuten.
                while (!loadTask.IsCompleted)
                {
                    Application.DoEvents();
                }

                await loadTask;

                // Assert
                Assert.IsNull(form.LastMessageShown, "Una carga exitosa no debe mostrar diálogos de error.");
                Assert.IsNotNull(form.LoadedDataTable, "El DataTable interno debería haberse llenado tras la carga asíncrona.");
                Assert.HasCount(2, form.GetGridPreview.Rows, "El Grid de previsualización debería tener 2 filas.");
                Assert.IsTrue(form.GetImportButton.Enabled, "Con filas cargadas el botón de importar debe habilitarse.");
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
        public void Import_ResultData_ShouldBeTypedAndOwn_Table()
        {
            // El resultado debe ser una tabla propia y tipada: sin columnas de control
            // del ImportManager y con los tipos de destino resueltos, de modo que el
            // consumidor pueda usarla incluso después de que el asistente se disponga.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();
                form.LoadData();
                form.SimulateImport();

                Assert.AreEqual(DialogResult.OK, form.DialogResult, "La importación con datos válidos debería cerrar con OK.");
                var result = form.ResultData;
                Assert.IsNotNull(result, "ResultData no debería ser nulo tras una importación exitosa.");
                Assert.IsNull(result.Columns["_RowIndex"], "El resultado no debe exponer la columna de control '_RowIndex'.");
                Assert.IsNull(result.Columns["_IsValid"], "El resultado no debe exponer la columna de control '_IsValid'.");
                var ageColumn = result.Columns["Age"];
                Assert.IsNotNull(ageColumn, "La columna 'Age' debería existir en el resultado tipado.");
                Assert.AreEqual(typeof(int), ageColumn.DataType, "La columna 'Age' debe ser tipada (int), no string.");
                Assert.AreEqual(30, result.Rows[0]["Age"], "El valor debería estar convertido al tipo de destino.");

                // La tabla es propiedad del llamador: debe seguir siendo legible
                // incluso después de disponer el asistente (que dispone sus recursos internos).
                form.Dispose();
                Assert.AreEqual(2, result.Rows.Count, "El resultado debe seguir siendo legible tras disponer el asistente.");
                Assert.AreEqual(30, result.Rows[0]["Age"], "Los valores tipados deben conservarse tras disponer el asistente.");
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
        public void Import_EmptyFile_ShouldWarn_AndNotExposeErrorsGridNoise()
        {
            // Un origen sin filas no es un error de validación: el wizard debe
            // avisar claramente ("no contiene filas") sin llenar la pestaña de
            // errores con ruido de validación ni habilitar la importación.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();

                // Cargar un origen sin filas (solo encabezados).
                form.SimulateEmptyLoadData();
                Assert.IsFalse(form.GetImportButton.Enabled,
                    "Con cero filas cargadas el botón de importar debe permanecer deshabilitado.");

                form.SimulateImport();

                StringAssert.Contains(form.LastMessageShown, "no contiene filas");
                Assert.AreEqual(MessageBoxIcon.Warning, form.LastMessageIcon,
                    "El aviso de archivo sin filas debe mostrarse como advertencia.");
                Assert.IsNull(form.ResultData, "No debe exponerse resultado para un origen vacío.");
                Assert.AreNotEqual(DialogResult.OK, form.DialogResult, "El wizard no debe cerrarse con OK.");
                Assert.AreEqual(0, form.GetGridErrors.Rows.Count,
                    "El grid de errores no debe llenarse con ruido para un archivo sin filas.");
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
        public void BuildActiveDefinitions_ShouldNotMutate_OriginalDefinitions()
        {
            // Arrange: la colección original pertenece al consumidor del wizard; el
            // mapeo activo debe ser una traducción interna, nunca un efecto lateral.
            var defs = GetSampleDefinitions();
            var originalAgeSource = defs["Age"].SourceColumnName;
            var originalNameSource = defs["Name"].SourceColumnName;

            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();

                // Carga con columna distinta y corrección del mapeo por el usuario:
                // es el escenario que dispara la reasignación de SourceColumnName.
                form.SimulateWithErrorColumnNameLoadData();
                form.SimulateCorrectingMapping();
                form.SimulateImport();

                Assert.AreEqual(originalAgeSource, defs["Age"].SourceColumnName,
                    "BuildActiveDefinitions no debe mutar la definición original: el consumidor reutiliza su colección (p. ej. en otra importación) y esperaría su configuración intacta.");
                Assert.AreEqual(originalNameSource, defs["Name"].SourceColumnName,
                    "BuildActiveDefinitions no debe mutar la definición original: el consumidor reutiliza su colección (p. ej. en otra importación) y esperaría su configuración intacta.");
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
        public void ErrorsGrid_ShouldShowDisplayName_ForFieldName()
        {
            // El usuario piensa en campos de negocio, no en identificadores internos:
            // la columna "Campo" del grid de errores debe mostrar el DisplayName
            // ("Edad"), nunca el FieldName ("Age") que usa la capa de validación.
            var defs = GetSampleDefinitions(true);
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();
                form.LoadData();
                form.SimulateImport();

                Assert.HasCount(1, form.GetGridErrors.Rows,
                    "Precondición: la regla LessThan(30) debe marcar exactamente la fila de 'Juan Perez' (30 años).");
                Assert.AreEqual("Edad", form.GetGridErrors.Rows[0].Cells["Campo"].Value,
                    "La columna Campo debe resolver el DisplayName del campo para el usuario final.");
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
        [STATestMethod]
        public void FilterHasErrors_ShouldShowOnlyErrorRows_AndEditsPropagate()
        {
            // El filtro solo-errores debe ser una VISTA sobre la misma tabla (DataView):
            // si se copian filas a una tabla temporal, las correcciones hechas en la
            // vista filtrada nunca llegan a la tabla subyacente y se pierden al revalidar.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();
                form.SimulateWithErrorLoadData();
                form.SimulateImport(); // Falla: "25x" queda marcado con error en la tabla cargada.

                Assert.IsTrue(form.LoadedDataTable!.Rows[1].HasErrors,
                    "El error de validación debe quedar pintado sobre la tabla proporcionada.");
                Assert.IsNull(form.ResultData, "Con errores de validación no debe exponerse resultado.");

                // Act 1: activar el filtro deja visible únicamente la fila con error.
                form.InvokeFilterHasErrors(true);
                Assert.AreEqual(1, form.GetGridPreview.Rows.Count,
                    "El filtro solo-errores debe mostrar únicamente las filas con error.");

                // Act 2: el usuario corrige la celda DESDE la vista filtrada y se revalida en silencio.
                form.GetGridPreview.Rows[0].Cells[1].Value = "25";
                form.SimulateCellEditRevalidation();

                Assert.IsFalse(form.LoadedDataTable.Rows[1].HasErrors,
                    "La corrección hecha en la vista filtrada debe propagarse a la tabla subyacente.");
                Assert.IsNotNull(form.ResultData,
                    "Tras corregir desde la vista filtrada, la revalidación debe exponer el resultado.");
                Assert.AreEqual(0, form.GetGridPreview.Rows.Count,
                    "La fila corregida debe salir de la vista filtrada al dejar de tener errores.");

                // Act 3: desactivar el filtro restaura la vista completa.
                form.InvokeFilterHasErrors(false);
                Assert.AreEqual(2, form.GetGridPreview.Rows.Count,
                    "Sin filtro deben volver a verse todas las filas de la tabla cargada.");
            }
        }
        [STATestMethod]
        public void MappingChange_ShouldTrigger_SilentRevalidationPath()
        {
            // Cambiar el mapeo de una columna origen programa (no ejecuta) una
            // revalidación silenciosa: sin diálogos y coalescida por el debounce.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ClearLastMessage();
                form.ShowOpenDialogFile();
                form.SimulateWithErrorColumnNameLoadData(); // El mapeo de Edad queda "(Ignorar)".
                try
                {
                    // Act: el usuario mapea Edad hacia "Columna1".
                    form.SimulateMappingChange();

                    Assert.IsNull(form.ResultData,
                        "La revalidación debe diferirse: nada debe ejecutarse de inmediato al cambiar el mapeo.");

                    // Esperar la ventana del debounce (400 ms) bombeando mensajes
                    // para que el temporizador dispare la revalidación.
                    var deadline = DateTime.UtcNow.AddSeconds(3);
                    while (form.ResultData is null && DateTime.UtcNow < deadline)
                    {
                        Application.DoEvents();
                        Thread.Sleep(25);
                    }

                    Assert.IsNotNull(form.ResultData,
                        "El debounce debió ejecutar la revalidación y exponer el resultado del mapeo corregido.");
                    Assert.IsNull(form.LastMessageShown,
                        "La revalidación programada por cambio de mapeo debe ser silenciosa: sin diálogos.");
                    Assert.HasCount(2, form.ResultData.Rows,
                        "El mapeo corregido debe producir un resultado con todas las filas.");
                }
                finally
                {
                    if (!form.IsDisposed) form.Close();
                }
            }
        }
        [STATestMethod]
        public void OptionsChanged_ShouldMarkDataStale_AndDisableImport()
        {
            // Cambiar opciones de análisis tras cargar datos marca los datos como
            // obsoletos: se invalida el resultado y se exige recargar antes de
            // importar (la recarga automática se descartó: pisaría ediciones sin aviso).
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.Show(); // La visibilidad de un ToolStripItem depende de su contenedor.
                try
                {
                    form.ClearLastMessage();
                    form.ShowOpenDialogFile();
                    form.LoadData();
                    // La revalidación silenciosa expone ResultData sin cerrar el formulario
                    // (a diferencia de importar con AutoCloseOnSuccess).
                    form.SimulateCellEditRevalidation();

                    Assert.IsNotNull(form.ResultData,
                        "Precondición: la revalidación con datos válidos debe exponer un resultado.");
                    Assert.IsTrue(form.GetImportButton.Enabled,
                        "Precondición: con datos cargados el botón de importar debe estar habilitado.");

                    // Act: el usuario cambia una opción de análisis.
                    form.SimulateOptionsChanged();

                    Assert.IsFalse(form.GetImportButton.Enabled,
                        "Tras un cambio de opciones los datos cargados son obsoletos: importar debe deshabilitarse.");
                    Assert.IsTrue(form.GetWarningLabel.Visible,
                        "El asistente debe avisar que la configuración cambió y hay que recargar los datos.");
                    StringAssert.Contains(form.GetWarningLabel.Text, "recargue",
                        "El aviso debe guiar al usuario a recargar los datos.");
                    Assert.IsNull(form.ResultData,
                        "El resultado previo fue construido con las opciones anteriores: debe invalidarse.");
                }
                finally
                {
                    if (!form.IsDisposed) form.Close();
                }
            }
        }
        [STATestMethod]
        public void FileName_Empty_ShouldResetWizardState()
        {
            // Asignar un FileName vacío (el consumidor limpia la selección) devuelve
            // al asistente a su estado inicial: sin preview, sin mapeo, sin
            // resultado y sin botones activos.
            var defs = GetSampleDefinitions();
            using (var form = new TestableImportWizardForm(defs))
            {
                var handle = form.Handle; // Forzar inicialización de controles
                form.ShowOpenDialogFile();
                form.LoadData();
                // La revalidación silenciosa expone ResultData sin cerrar el formulario.
                form.SimulateCellEditRevalidation();

                Assert.IsNotNull(form.ResultData, "Precondición: debe existir un resultado antes del reinicio.");
                Assert.IsTrue(form.GetImportButton.Enabled, "Precondición: el botón de importar debe estar habilitado.");
                Assert.AreNotEqual(0, form.GetGridPreview.Rows.Count, "Precondición: el preview debe tener filas.");

                // Act: limpiar el archivo seleccionado.
                form.FileName = string.Empty;

                Assert.AreEqual(string.Empty, form.FileName, "La ruta debe quedar vacía.");
                Assert.IsNull(form.ResultData, "El reinicio debe descartar el resultado previo.");
                Assert.AreEqual(0, form.GetGridPreview.Rows.Count, "El preview debe quedar vacío.");
                Assert.AreEqual(0, form.GetGridMapping.Rows.Count, "El mapeo debe quedar limpio.");
                Assert.IsFalse(form.GetImportButton.Enabled, "Sin datos cargados no debe poder importarse.");
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
