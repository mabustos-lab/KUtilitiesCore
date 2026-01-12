using DocumentFormat.OpenXml.Drawing.Charts;
using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml;
using KUtilitiesCore.Data.DataImporter.Infrastructure;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KUtilitiesCore.Data.Win.Importer
{
    public partial class ImportWizardForm : Form
    {
        #region Fields

        private readonly FielDefinitionCollection _fieldDefinitions;

        private readonly ImportManager _importManager;

        // Estado Lógico
        private IImportConfigControl currentConfigControl;

        private System.Data.DataTable loadedDataTable;

        #endregion Fields

        #region Constructors

        public ImportWizardForm(FielDefinitionCollection fieldDefinitions, ImportManager importManager = null)
        {
            InitializeComponent();

            _fieldDefinitions = fieldDefinitions ?? [];
            _importManager = importManager ?? new ImportManager();
            ResultData = new System.Data.DataTable();
            dgvMapping.AutoGenerateColumns = false;
            dgvPreview.AutoGenerateColumns = false;
        }

        #endregion Constructors

        #region Properties

        public string FileName
        {
            get => txtFilePath.Text;
            set => txtFilePath.Text = value;
        }

        // Propiedad pública para leer los datos cargados (útil para tests)
        public System.Data.DataTable LoadedDataTable => loadedDataTable;

        public System.Data.DataTable ResultData { get; private set; }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Iniicializa elproceso para la carga de información
        /// </summary>
        public virtual void LoadData()
        {
            tsslWarning.Visible = false;
            tsslCount.Text = "Filas cargadas: 0";
            if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show("Seleccione un archivo primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                IDataSourceReader reader = null;
                var options = currentConfigControl.GetParsingOptions();

                if (cboSourceType.SelectedItem.ToString() == "CSV")
                {
                    // Nota: Asumiendo que DefaultDiskFileReader existe e implementa IDiskFileReader
                    reader = CsvSourceReaderFactory.CreateWithOptions(txtFilePath.Text, (TextFileParsingOptions)options);
                }
                else
                {
                    // Nota: Asumiendo que ClosedXmlWorkbookReaderFactory existe
                    reader = ExcelSourceReaderFactory.CreateWithOptions(
                        txtFilePath.Text,
                        (ExcelParsingOptions)options,
                        new ClosedXmlWorkbookReaderFactory());
                }

                loadedDataTable = reader.ReadData();
                dgvPreview.DataSource = loadedDataTable;

                PopulateMappingGrid();

                btnImport.Enabled = true;
                tsslCount.Text = $"Filas cargadas: {loadedDataTable.Rows.Count:N0}";
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                tsslWarning.Text = "Error al leer el archivo.";
                tsslWarning.Visible = true;
                MessageBox.Show(
                    $"Error al leer el archivo: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public virtual void ImportData()
        {
            // 1. Construir colección de definiciones activas con el mapeo actualizado
            var activeDefinitions = new FielDefinitionCollection();

            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                // Recuperamos la definición original desde el Tag
                var originalDef = row.Tag as FieldDefinition;
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
                _importManager.ReadData(loadedDataTable);
                // El ImportManager ahora usará 'SourceColumnName' de cada definición para buscar en
                // 'loadedDataTable' No es necesario renombrar columnas en el DataTable. var result
                // = _importManager.LoadData(loadedDataTable, activeDefinitions);
                ValidateData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico en el proceso de importación: {ex.Message}", "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }

        private void ValidateData()
        {
            _importManager.ValidateDataTypes();
            if (_importManager.ValidationErrors.IsValid)
            {
                // Éxito
                ResultData = _importManager.DataSource;
                MessageBox.Show("Importación completada y validada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                // Mostrar errores
                MessageBox.Show($"Se encontraron {_importManager.ValidationErrors.Errors.Count} errores de validación.", "Errores", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Llenar grid de errores
                dgvErrors.DataSource = null;
                dgvErrors.DataSource = _importManager.ValidationErrors.Errors.Select(err =>
                {
                    var failure = err as ValidationFailure;
                    return new
                    {
                        // Convertimos a string para asegurar consistencia de tipo en la lista anónima (int vs string "-")
                        Fila = failure != null ? failure.IndexRow.ToString() : "-",
                        Campo = failure != null ? failure.PropertyName : "General",
                        Mensaje = err.ErrorMessage,
                        ValorIntentado = failure != null ? failure.AttemptedValue : null
                    };
                }).ToList();

                tabControlResults.SelectedTab = tabControlResults.TabPages[1]; // Ir a tab errores

                // No cerramos el form para permitir correcciones
                this.DialogResult = DialogResult.None;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                // Filtro unificado que acepta todos los formatos soportados
                ofd.Filter = "Todos los archivos soportados|*.csv;*.tsv;*.psv;*.txt;*.xlsx;*.xls|Archivos de Texto (*.csv, *.tsv, *.psv, *.txt)|*.csv;*.tsv;*.psv;*.txt|Excel (*.xlsx, *.xls)|*.xlsx;*.xls";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;

                    // Lógica para detectar el tipo de archivo y cambiar el modo automáticamente
                    string ext = Path.GetExtension(ofd.FileName).ToLower();

                    if (ext == ".xlsx" || ext == ".xls")
                    {
                        cboSourceType.SelectedItem = "Excel";
                    }
                    else
                    {
                        // Asumimos CSV/Texto para .csv, .tsv, .psv, .txt
                        cboSourceType.SelectedItem = "CSV";
                    }

                    // Forzamos actualización de UI para asegurar que el control se ha creado
                    Application.DoEvents();

                    // Ahora inicializamos el control con la ruta del archivo (esto puede
                    // preconfigurar delimitadores)
                    currentConfigControl.Initialize(ofd.FileName);
                    btnLoadData.Enabled = true;
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ImportData();
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void cboSourceType_SelectedIndexChanged(object sender, EventArgs e)
        { SourceTypeUpdate(); }

        private void PopulateMappingGrid()
        {
            dgvMapping.Rows.Clear();

            // Obtener columnas del DataTable cargado
            var sourceColumns = new List<string> { "(Ignorar)" };
            foreach (DataColumn col in loadedDataTable.Columns)
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
                if (string.IsNullOrEmpty(expectedSource)) expectedSource = field.DisplayName;
                if (string.IsNullOrEmpty(expectedSource)) expectedSource = field.ColumnName;

                // Intentar buscar coincidencia insensible a mayúsculas/minúsculas en las columnas
                // del archivo
                var match = sourceColumns.FirstOrDefault(c => c.Equals(expectedSource, StringComparison.OrdinalIgnoreCase))
                            ?? "(Ignorar)";

                row.Cells[1].Value = match;
            }
        }

        private void SourceTypeUpdate()
        {
            pnlConfig.Controls.Clear();
            if (cboSourceType.SelectedItem?.ToString() == "CSV")
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

        #endregion Methods
    }
}