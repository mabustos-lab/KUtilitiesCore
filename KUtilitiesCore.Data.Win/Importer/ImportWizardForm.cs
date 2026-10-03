using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.ComponentModel;

namespace KUtilitiesCore.Data.Win.Importer
{
    /// <summary>
    /// Asistente WinForms de importación de datos: guía al usuario en la selección del
    /// archivo, la configuración del análisis (CSV/Excel), el mapeo de columnas y la
    /// validación, dejando el resultado en <see cref="ResultData"/> cuando todos los
    /// datos son válidos.
    /// </summary>
    public partial class ImportWizardForm : Form
    {
        #region Fields

        private readonly FieldDefinitionCollection _fieldDefinitions;

        private readonly ImportManager _importManager;

        // Estado Lógico
        private IImportConfigControl? currentConfigControl;

        private string? currentExtFile;

        private System.Data.DataTable? loadedDataTable;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa el asistente con las definiciones de campos destino y, opcionalmente,
        /// un <see cref="ImportManager"/> ya configurado; si no se proporciona se crea uno
        /// interno.
        /// </summary>
        /// <param name="fieldDefinitions">Definiciones de campos que regirán el mapeo y la validación.</param>
        /// <param name="importManager">Gestor de importación a utilizar; si es <c>null</c> se instancia uno nuevo.</param>
        public ImportWizardForm(FieldDefinitionCollection fieldDefinitions, ImportManager? importManager = null)
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoCloseOnSuccess { get; set; } = true;

        /// <summary>
        /// Establece el nombre del archivo que se desea importar.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Data.DataTable? LoadedDataTable
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
            if (currentConfigControl is null)
            {
                ShowMessage("No hay configuración de importación disponible.", "Aviso", MessageBoxIcon.Warning);
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                IDataSourceReader? reader = null;
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
                ofd.DefaultExt = ".xlsx";
                ofd.AddExtension = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileName = ofd.FileName;
                }
            }
        }

        /// <summary>
        /// Importa los datos cargados al DataTable final. El cierre del formulario
        /// lo decide exclusivamente este método según el resultado de la validación:
        /// con éxito cierra con <see cref="DialogResult.OK"/> (si
        /// <see cref="AutoCloseOnSuccess"/>); con errores permanece abierto para que
        /// el usuario corrija los datos.
        /// </summary>
        protected virtual void ImportData()
        {
            var sourceTable = LoadedDataTable;
            if (sourceTable is null)
            {
                ShowMessage("No hay datos cargados para importar.", "Aviso", MessageBoxIcon.Warning);
                return;
            }

            var activeDefinitions = BuildActiveDefinitions();
            try
            {
                _importManager.SetMapping(activeDefinitions);
                _importManager.ReadData(sourceTable);
                _importManager.ValidateDataTypes();

                // El proceso se notifica antes de decidir el cierre para que derivaciones
                // puedan abortar (lanzando una excepción) si su lógica lo requiere.
                OnProcessImportFinished();

                if (_importManager.ValidationErrors.IsValid)
                {
                    ResultData = _importManager.DataSource;
                    RefreshErrorsGrid();
                    tsslWarning.Visible = false;
                    if (AutoCloseOnSuccess)
                    {
                        // Cierre automático silencioso: la ventana desaparece cuando los
                        // datos quedaron correctamente importados.
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        ShowMessage(
                            "Importación completada y validada correctamente.",
                            "Éxito",
                            MessageBoxIcon.Information);
                    }
                }
                else
                {
                    ResultData = null;
                    ShowMessage(
                        $"Se encontraron {_importManager.ValidationErrors.Errors.Count} errores de validación.",
                        "Errores",
                        MessageBoxIcon.Warning);
                    RefreshErrorsGrid();
                    tsslWarning.Text = "Los datos contienen errores, favor de verificar.";
                    tsslWarning.Visible = true;
                    tabControlResults.SelectedTab = tabControlResults.TabPages[1];

                    // No cerramos el form para permitir correcciones
                    DialogResult = DialogResult.None;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error crítico en el proceso de importación: {ex.Message}",
                    "Error Crítico", MessageBoxIcon.Error);

                // El fallo queda registrado persistentemente en la barra de estado; un
                // MessageBox es transitorio y se descarta con un clic, el aviso no.
                tsslWarning.Text = "Error crítico en la importación, favor de verificar.";
                tsslWarning.Visible = true;
                DialogResult = DialogResult.None;
            }
        }

        /// <summary>
        /// Revalida silenciosamente los datos tras una edición en el grid de
        /// previsualización: refresca el grid de errores y el aviso de la barra de
        /// estado sin mostrar diálogos, sin cambiar de pestaña y sin cerrar el
        /// formulario, para no interrumpir al usuario mientras corrige celda por celda.
        /// </summary>
        protected virtual void RevalidateAfterEdit()
        {
            var sourceTable = LoadedDataTable;
            if (sourceTable is null) return;

            var activeDefinitions = BuildActiveDefinitions();
            try
            {
                _importManager.SetMapping(activeDefinitions);
                _importManager.ReadData(sourceTable);
                _importManager.ValidateDataTypes();
                OnProcessImportFinished();

                if (_importManager.ValidationErrors.IsValid)
                {
                    ResultData = _importManager.DataSource;
                    RefreshErrorsGrid();
                    tsslWarning.Visible = false;
                }
                else
                {
                    ResultData = null;
                    RefreshErrorsGrid();
                    tsslWarning.Text = "Los datos contienen errores, favor de verificar.";
                    tsslWarning.Visible = true;
                }
            }
            catch
            {
                // La revalidación por edición es de fondo: solo deja el aviso persistente;
                // el diagnóstico detallado se reserva para el botón Importar.
                tsslWarning.Text = "Error crítico en la importación, favor de verificar.";
                tsslWarning.Visible = true;
            }
        }

        /// <summary>
        /// Construye la colección de definiciones activas a partir del mapeo actual
        /// del grid de mapeo; punto único de traducción entre la UI y el ImportManager.
        /// </summary>
        private FieldDefinitionCollection BuildActiveDefinitions()
        {
            var activeDefinitions = new FieldDefinitionCollection();
            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                var originalDef = row.Tag as FieldDefinitionItem;
                string selectedSourceCol = row.Cells[1].Value?.ToString()!;

                if (originalDef is null) continue;

                if (selectedSourceCol != "(Ignorar)" && !string.IsNullOrEmpty(selectedSourceCol))
                {
                    originalDef.SourceColumnName = selectedSourceCol;
                }
                else
                {
                    // Nombre ficticio: el ImportManager no encontrará la columna y el
                    // valor extraído será null (equivale a ignorar el campo).
                    originalDef.SourceColumnName = originalDef.DisplayName;
                }
                activeDefinitions.Add(originalDef);
            }
            return activeDefinitions;
        }

        /// <summary>
        /// Volca los errores de validación actuales al grid de errores; con validación
        /// exitosa el grid queda vacío, de modo que la UI nunca muestra errores obsoletos.
        /// </summary>
        private void RefreshErrorsGrid()
        {
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
        }

        /// <summary>
        /// Ocurre cuando el archivo es cargado y asignado al DataSource
        /// </summary>
        protected virtual void OnLoadedDataSource()
        {
            var table = loadedDataTable;
            if (table is null) return;

            dgvPreview.DataSource = table;
            ResultData = null;

            PopulateMappingGrid();
            btnImport.Enabled = true;
            tsslCount.Text = $"Filas cargadas: {table.Rows.Count:N0}";
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

        /// <summary>
        /// Muestra un mensaje al usuario; se declara virtual para que pruebas y
        /// derivaciones puedan capturarlo en lugar de presentar un diálogo modal.
        /// </summary>
        /// <param name="message">Texto del mensaje.</param>
        /// <param name="caption">Título de la ventana.</param>
        /// <param name="msgIcon">Icono que refleja la severidad del mensaje.</param>
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
            var table = loadedDataTable;
            if (table is null) return;
            FilterHasErrors(table, dgvPreview, cbFilterHasError.Checked);
        }

        private void dgvPreview_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // La edición del preview solo revalida en silencio; la decisión de cerrar
            // corresponde exclusivamente al botón Importar.
            RevalidateAfterEdit();
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
            loadedDataTable?.Dispose();
            ResultData?.Dispose();
            if (_importManager != null)
                _importManager.Dispose();
        }

        private void OnSelectedFile()
        {
            // Lógica para detectar el tipo de archivo y cambiar el modo automáticamente
            currentExtFile = Path.GetExtension(txtFilePath.Text).ToLower();
            btnLoadData.Enabled = false;
            var configControl = currentExtFile == ".xlsx" || currentExtFile == ".xls"
                ? SourceTypeUpdate("Excel")
                : SourceTypeUpdate("CSV");
            currentConfigControl = configControl;

            // Forzamos actualización de UI para asegurar que el control se ha creado
            Application.DoEvents();

            // Ahora inicializamos el control con la ruta del archivo (esto puede preconfigurar delimitadores)
            configControl.Initialize(txtFilePath.Text);
            btnLoadData.Enabled = true;
        }

        private void PopulateMappingGrid()
        {
            var table = loadedDataTable;
            if (table is null) return;

            dgvMapping.Rows.Clear();

            // Obtener columnas del DataTable cargado
            var sourceColumns = new List<string> { "(Ignorar)" };

            foreach (DataColumn col in table.Columns)
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
                string displayLabel = !string.IsNullOrEmpty(field.DisplayName) ? field.DisplayName : field.FieldName;

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
                    expectedSource = field.FieldName;

                // Intentar buscar coincidencia insensible a mayúsculas/minúsculas en las columnas
                // del archivo
                var match = sourceColumns.FirstOrDefault(
                        c => c.Equals(expectedSource, StringComparison.OrdinalIgnoreCase)) ??
                    "(Ignorar)";

                row.Cells[1].Value = match;
            }
        }

        /// <summary>
        /// Crea el control de configuración adecuado para el tipo de fuente indicado,
        /// lo hospeda en el panel de configuración y lo devuelve al llamador.
        /// </summary>
        /// <param name="sourceTypeTagSelected">Etiqueta del tipo de fuente ("Excel" o "CSV").</param>
        /// <returns>Control de configuración recién creado y ya agregado a la UI.</returns>
        private IImportConfigControl SourceTypeUpdate(string sourceTypeTagSelected)
        {
            if (string.IsNullOrEmpty(sourceTypeTagSelected))
                throw new ArgumentNullException(nameof(sourceTypeTagSelected));
            pnlConfig.Controls.Clear();
            IImportConfigControl configControl;
            if (sourceTypeTagSelected.Equals("csv", StringComparison.InvariantCultureIgnoreCase))
            {
                configControl = new CsvConfigControl();
            }
            else
            {
                configControl = new ExcelConfigControl();
            }

            var ctrl = (Control)configControl;
            ctrl.Dock = DockStyle.Fill;
            pnlConfig.Controls.Add(ctrl);
            return configControl;
        }

        #endregion Methods
    }
}