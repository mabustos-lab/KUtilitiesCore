using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;

namespace KUtilitiesCore.Data.Win.Importer
{
    public partial class ImportWizardForm : Form
    {
        #region Fields

        private readonly FieldDefinitionCollection _fieldDefinitions;

        private readonly ImportManager _importManager;

        // Estado Lógico
        private IImportConfigControl currentConfigControl;

        private string currentExtFile;

        private System.Data.DataTable loadedDataTable;

        #endregion Fields

        #region Constructors

        public ImportWizardForm(FieldDefinitionCollection fieldDefinitions, ImportManager importManager = null)
        {
            InitializeComponent();

            _fieldDefinitions = fieldDefinitions ?? [];
            _importManager = importManager ?? new ImportManager();
            ResultData = null;
            dgvMapping.AutoGenerateColumns = false;
            //dgvPreview.AutoGenerateColumns = false;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Establece si la ventana se cerrara cuando la importación esta completada y validada correctamente.
        /// </summary>
        public bool AutoCloseOnSuccess { get; set; } = true;

        /// <summary>
        /// Establece el nombre del archivo que se desea importar.
        /// </summary>
        public string FileName
        {
            get => txtFilePath.Text; set
            {
                txtFilePath.Text = value;
                if (!string.IsNullOrEmpty(value))
                    OnSelectedFile();
            }
        }

        /// <summary>
        /// Datos cargados de la fuente de datos.
        /// </summary>
        public System.Data.DataTable LoadedDataTable
        {
            get => loadedDataTable;
            protected set
            {
                loadedDataTable = value;
                OnLoadedDataSource();
            }
        }

        /// <summary>
        /// Datos requeridos para la importación
        /// </summary>
        public System.Data.DataTable? ResultData { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Iniicializa el proceso para la carga de información
        /// </summary>
        public virtual void LoadData()
        {
            tsslWarning.Visible = false;
            tsslCount.Text = "Filas cargadas: 0";
            if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                ShowMessage("Seleccione un archivo primero.", "Aviso", MessageBoxIcon.Warning);
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                IDataSourceReader reader = null;
                var options = currentConfigControl.GetParsingOptions();

                if (currentExtFile == ".xlsx" || currentExtFile == ".xls")
                {
                    reader = ExcelSourceReaderFactory.CreateWithOptions(
                        txtFilePath.Text,
                        (ExcelParsingOptions)options,
                        new ClosedXmlWorkbookReaderFactory());
                }
                else
                {
                    reader = CsvSourceReaderFactory.CreateWithOptions(txtFilePath.Text, (TextFileParsingOptions)options);
                }

                LoadedDataTable = reader.ReadData();

                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                tsslWarning.Text = "Error al leer el archivo.";
                tsslWarning.Visible = true;
                ShowMessage(
                    $"Error al leer el archivo: {ex.Message}",
                    "Error",
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Muestra un cuado de dialogo para selecionar el Arcivo de fuente de datos.
        /// </summary>
        public virtual void ShowOpenDialogFile()
        {
            using (var ofd = new OpenFileDialog())
            {
                // Filtro unificado que acepta todos los formatos soportados
                ofd.Filter = "Todos los archivos soportados|*.csv;*.tsv;*.psv;*.txt;*.xlsx;*.xls|Archivos de Texto (*.csv, *.tsv, *.psv, *.txt)|*.csv;*.tsv;*.psv;*.txt|Excel (*.xlsx, *.xls)|*.xlsx;*.xls";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileName = ofd.FileName;
                }
            }
        }

        /// <summary>
        /// Importamos el los datos cargados al DataTable final
        /// </summary>
        protected virtual void ImportData()
        {
            // 1. Construir colección de definiciones activas con el mapeo actualizado

            var activeDefinitions = new FieldDefinitionCollection();

            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                // Recuperamos la definición original desde el Tag
                var originalDef = row.Tag as FieldDefinitionItem;
                string selectedSourceCol = row.Cells[1].Value?.ToString()!;

                if (originalDef != null && selectedSourceCol != "(Ignorar)" && !string.IsNullOrEmpty(selectedSourceCol))
                {
                    // Actualizamos la propiedad SourceColumnName del objeto definición. Esto le
                    // indica al ImportManager exactamente qué columna del DataTable debe leer para
                    // este campo.
                    // NOTA: Esto modifica la instancia en memoria. Si se desea preservar la
                    // configuración original intacta para reutilización futura sin estos cambios,
                    // se debería clonar el objeto aquí. Dado el contexto de UI, modificar la
                    // instancia para alinearla con la selección del usuario es el comportamiento esperado.
                    originalDef.SourceColumnName = selectedSourceCol;

                    activeDefinitions.Add(originalDef);
                }
            }

            // 2. Ejecutar ImportManager
            try
            {
                _importManager.SetMapping(activeDefinitions);
                _importManager.ReadData(LoadedDataTable);
                
                ValidateData();
                OnProcessImportFinished();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error crítico en el proceso de importación: {ex.Message}",
                    "Error Crítico", MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }

        /// <summary>
        /// Ocurre cuando el archivo es cargado y asignado al DataSource
        /// </summary>
        protected virtual void OnLoadedDataSource()
        {
            dgvPreview.DataSource = LoadedDataTable;
            ResultData = null;

            PopulateMappingGrid();
            btnImport.Enabled = true;
            tsslCount.Text = $"Filas cargadas: {LoadedDataTable.Rows.Count:N0}";
        }

        /// <summary>
        /// Ocurre cuando finaliza el proceso de importación de datos
        /// </summary>
        protected virtual void OnProcessImportFinished()
        {
            if (_importManager != null && _importManager.ValidationErrors.Errors.Count > 0)
            {
                tsslWarning.Text = "Los datos contienen errores, favor de verificar.";
                tsslWarning.Visible = true;
            }
        }

        protected virtual void ShowMessage(string message, string caption, MessageBoxIcon msgIcon)
        {
            MessageBox.Show(
                  message,
                   caption,
                   MessageBoxButtons.OK,
                   msgIcon);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ShowOpenDialogFile();
        }

        private void btnImport_Click(object sender, EventArgs e)
        { ImportData(); }

        private void btnLoadData_Click(object sender, EventArgs e)
        { LoadData(); }

        private void cbFilterHasError_CheckedChanged(object sender, EventArgs e)
        {
            FilterHasErrors(LoadedDataTable, dgvPreview, cbFilterHasError.Checked);
        }

        private void dgvPreview_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ImportData();
        }

        private void FilterHasErrors(DataTable dt, DataGridView dgv, bool showErrorOnly)
        {
            if (showErrorOnly)
            {
                // Seleccionamos solo las filas con error
                var rowsConError = dt.Rows.Cast<DataRow>()
                                          .Where(r => r.HasErrors);

                if (rowsConError.Any())
                {
                    // Creamos una vista temporal con esas filas
                    DataTable dtErrores = dt.Clone(); // misma estructura
                    foreach (var row in rowsConError)
                        dtErrores.ImportRow(row);

                    // ImportRow mantiene la referencia a la fila original, así que las ediciones se
                    // reflejan en el DataTable base
                    dgv.DataSource = dtErrores;
                }
                else
                {
                    // Si no hay errores, mostramos tabla vacía con misma estructura
                    dgv.DataSource = dt.Clone();
                }
            }
            else
            {
                // Restauramos la vista completa
                dgv.DataSource = dt;
            }
        }

        private void OnDispose()
        {
            LoadedDataTable.Dispose();
            ResultData.Dispose();
            if (_importManager != null)
                _importManager.Dispose();
        }

        private void OnSelectedFile()
        {
            // Lógica para detectar el tipo de archivo y cambiar el modo automáticamente
            currentExtFile = Path.GetExtension(txtFilePath.Text).ToLower();

            if (currentExtFile == ".xlsx" || currentExtFile == ".xls")
            {
                SourceTypeUpdate("Excel");
            }
            else
            {
                // Asumimos CSV/Texto para .csv, .tsv, .psv, .txt
                SourceTypeUpdate("CSV");
            }

            // Forzamos actualización de UI para asegurar que el control se ha creado
            Application.DoEvents();

            // Ahora inicializamos el control con la ruta del archivo (esto puede preconfigurar delimitadores)
            currentConfigControl.Initialize(txtFilePath.Text);
            btnLoadData.Enabled = true;
        }

        private void PopulateMappingGrid()
        {
            dgvMapping.Rows.Clear();

            // Obtener columnas del DataTable cargado
            var sourceColumns = new List<string> { "(Ignorar)" };

            foreach (DataColumn col in LoadedDataTable.Columns)
            {
                sourceColumns.Add(col.ColumnName);
            }

            // Configurar el ComboBox de la columna 1 (Source)
            var comboCol = (DataGridViewComboBoxColumn)dgvMapping.Columns[1];
            comboCol.DataSource = sourceColumns;

            // Crear filas para cada FieldDefinition
            foreach (var field in _fieldDefinitions)
            {
                int rowIndex = dgvMapping.Rows.Add();
                var row = dgvMapping.Rows[rowIndex];

                // 1. Configurar etiqueta UI Preferimos DisplayName. Si no existe, usamos ColumnName.
                string displayLabel = !string.IsNullOrEmpty(field.DisplayName) ? field.DisplayName : field.ColumnName;

                row.Cells[0].Value = displayLabel;
                row.Cells[0].ToolTipText = field.Description; // Usamos Description para tooltip

                // IMPORTANTE: Guardamos el objeto FieldDefinition en el Tag para recuperarlo
                // fácilmente después
                row.Tag = field;

                // 2. Lógica de auto-mapeo (Heurística) Buscamos coincidencia en este orden:
                // SourceColumnName -> DisplayName -> ColumnName
                string expectedSource = field.SourceColumnName;
                if (string.IsNullOrEmpty(expectedSource))
                    expectedSource = field.DisplayName;
                if (string.IsNullOrEmpty(expectedSource))
                    expectedSource = field.ColumnName;

                // Intentar buscar coincidencia insensible a mayúsculas/minúsculas en las columnas
                // del archivo
                var match = sourceColumns.FirstOrDefault(
                        c => c.Equals(expectedSource, StringComparison.OrdinalIgnoreCase)) ??
                    "(Ignorar)";

                row.Cells[1].Value = match;
            }
        }

        private void SourceTypeUpdate(string sourceTypeTagSelected)
        {
            if (string.IsNullOrEmpty(sourceTypeTagSelected))
                throw new ArgumentNullException(nameof(sourceTypeTagSelected));
            pnlConfig.Controls.Clear();
            if (sourceTypeTagSelected.Equals("csv", StringComparison.InvariantCultureIgnoreCase))
            {
                currentConfigControl = new CsvConfigControl();
            }
            else
            {
                currentConfigControl = new ExcelConfigControl();
            }

            var ctrl = (Control)currentConfigControl;
            ctrl.Dock = DockStyle.Fill;
            pnlConfig.Controls.Add(ctrl);
        }

        private void ValidateData()
        {
            _importManager.ValidateDataTypes();
            if (_importManager.ValidationErrors.IsValid)
            {
                // Éxito
                ResultData = _importManager.DataSource;
                ShowMessage(
                    "Importación completada y validada correctamente.",
                    "Éxito",
                    MessageBoxIcon.Information);
                if (AutoCloseOnSuccess)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            else
            {
                // Mostrar errores
                ShowMessage(
                    $"Se encontraron {_importManager.ValidationErrors.Errors.Count} errores de validación.",
                    "Errores",
                    MessageBoxIcon.Warning);

                // Llenar grid de errores
                dgvErrors.DataSource = null;
                dgvErrors.DataSource = _importManager.ValidationErrors.Errors
                    .Select(
                        err =>
                        {
                            var failure = err as ValidationFailure;
                            return new
                            {
                                // Convertimos a string para asegurar consistencia de tipo en la
                                // lista anónima (int vs string "-")
                                Fila = failure != null ? failure.IndexRow.ToString() : "-",
                                Campo = failure != null ? failure.PropertyName : "General",
                                Mensaje = err.ErrorMessage,
                                ValorIntentado = failure != null ? failure.AttemptedValue : null
                            };
                        })
                    .ToList();

                tabControlResults.SelectedTab = tabControlResults.TabPages[1]; // Ir a tab errores

                // No cerramos el form para permitir correcciones
                this.DialogResult = DialogResult.None;
            }
        }

        #endregion Methods
    }
}