using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Data;
using System.Linq;
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

        /// <summary>
        /// Indica si hay una carga asíncrona en curso; evita que el usuario dispare
        /// cargas reentrantes mientras una lectura sigue pendiente.
        /// </summary>
        private bool _isLoading;

        /// <summary>
        /// Temporizador que coalescea las ediciones del preview y del mapeo en una
        /// única revalidación silenciosa tras una ventana de 400 ms.
        /// </summary>
        /// <remarks>
        /// Se usa un <see cref="System.Windows.Forms.Timer"/> (y no validación en un
        /// hilo de background) porque el <see cref="DataTable"/> enlazado al grid vía
        /// <see cref="DataView"/> no es seguro para acceso multi-hilo: el temporizador
        /// mantiene todo el trabajo en el hilo de UI.
        /// </remarks>
        private readonly System.Windows.Forms.Timer _revalidateDebounce = new() { Interval = 400 };

        /// <summary>
        /// Indica si <see cref="PopulateMappingGrid"/> está poblando el grid de mapeo;
        /// suprime los eventos de cambio de valor generados programáticamente para no
        /// confundirlos con intención de usuario.
        /// </summary>
        private bool _populatingMapping;

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

            // El temporizador dispara la revalidación silenciosa acumulada; se
            // cancela/reinicia con cada edición (debounce) desde ScheduleSilentRevalidation.
            _revalidateDebounce.Tick += (sender, e) => RevalidateAfterEdit();
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
        /// <remarks>
        /// Asignar una ruta vacía reinicia el asistente (<see cref="ResetWizardState"/>):
        /// el consumidor puede limpiar la selección y la UI no debe conservar
        /// datos de un archivo ya descartado.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FileName
        {
            get => txtFilePath.Text; set
            {
                txtFilePath.Text = value;
                if (string.IsNullOrEmpty(value))
                    ResetWizardState();
                else
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
        /// Iniicializa el proceso para la carga de información.
        /// </summary>
        /// <remarks>
        /// El lector devuelto por <see cref="CreateReader"/> se dispone siempre en el bloque
        /// <c>finally</c>: los lectores de Excel mantienen el archivo físico abierto mientras
        /// el objeto viva, por lo que no disponerlo bloqueaba el archivo hasta que el GC
        /// actuara (el usuario no podía volver a guardar o reemplazar el archivo fuente).
        /// </remarks>
        public virtual void LoadData()
        {
            tsslWarning.Visible = false;
            tsslCount.Text = "Filas cargadas: 0";
            var reader = CreateReader();
            if (reader is null)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                LoadedDataTable = reader.ReadData();
            }
            catch (Exception ex)
            {
                HandleLoadError(ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                (reader as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        /// Versión asíncrona de <see cref="LoadData"/>: libera el subproceso de UI mientras
        /// el lector procesa el archivo y actualiza el pipeline de la vista al completarse.
        /// </summary>
        /// <remarks>
        /// Las continuaciones usan <c>ConfigureAwait(true)</c> (el comportamiento por defecto se
        /// conserva de forma explícita) porque tocan controles de UI (preview, mapeo, barra de
        /// estado) que exigen el hilo de UI. Reutiliza <see cref="CreateReader"/> y
        /// <see cref="HandleLoadError"/> para mantener una sola ruta de decisiones, y el lector
        /// se dispone en el bloque <c>finally</c> por la misma razón que en la carga síncrona:
        /// los lectores de Excel mantienen el archivo físico abierto hasta su disposición.
        /// <example>
        /// <code>
        /// // Desde el hilo de UI:
        /// await wizard.LoadDataAsync();
        /// </code>
        /// </example>
        /// </remarks>
        public virtual async Task LoadDataAsync()
        {
            tsslWarning.Visible = false;
            tsslCount.Text = "Filas cargadas: 0";
            var reader = CreateReader();
            if (reader is null)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var table = await reader.ReadDataAsync().ConfigureAwait(true);
                LoadedDataTable = table;
            }
            catch (Exception ex)
            {
                HandleLoadError(ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                (reader as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        /// Crea el lector de origen de datos adecuado para el archivo seleccionado
        /// según su extensión y la configuración actual del panel.
        /// </summary>
        /// <returns>
        /// El lector listo para leer, o <see langword="null"/> cuando falta el archivo
        /// o la configuración; en ese caso el usuario ya fue notificado con un aviso.
        /// </returns>
        /// <remarks>
        /// Se extrae como método virtual para que las pruebas y derivadas puedan
        /// sustituir el lector real (stub) sin acoplarse a las fábricas concretas.
        /// </remarks>
        protected virtual IDataSourceReader? CreateReader()
        {
            if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                ShowMessage("Seleccione un archivo primero.", "Aviso", MessageBoxIcon.Warning);
                return null;
            }

            if (currentConfigControl is null)
            {
                ShowMessage("No hay configuración de importación disponible.", "Aviso", MessageBoxIcon.Warning);
                return null;
            }

            var options = currentConfigControl.GetParsingOptions();
            if (currentExtFile == ".xlsx" || currentExtFile == ".xls")
            {
                return ExcelSourceReaderFactory.CreateWithOptions(
                    txtFilePath.Text,
                    (ExcelParsingOptions)options,
                    new ClosedXmlWorkbookReaderFactory());
            }

            return CsvSourceReaderFactory.CreateWithOptions(txtFilePath.Text, (TextFileParsingOptions)options);
        }

        /// <summary>
        /// Presenta al usuario un error ocurrido durante la lectura del archivo:
        /// indicador persistente en la barra de estado más un cuadro de diálogo.
        /// </summary>
        /// <param name="ex">Excepción capturada durante la lectura.</param>
        /// <remarks>
        /// La barra de estado permanece visible como recordatorio de que los datos
        /// mostrados provienen de una carga anterior, no del último intento fallido.
        /// </remarks>
        protected virtual void HandleLoadError(Exception ex)
        {
            tsslWarning.Text = "Error al leer el archivo.";
            tsslWarning.Visible = true;
            ShowMessage(
                $"Error al leer el archivo: {ex.Message}",
                "Error",
                MessageBoxIcon.Error);
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

            CommitPendingGridEdit();

            // Una revalidación diferida no debe dispararse a mitad (o al terminar) de
            // una importación explícita: se cancela la ventana pendiente del debounce.
            _revalidateDebounce.Stop();

            // Un origen vacío no es un error de validación: se avisa al usuario y
            // se sale sin llenar la pestaña de errores con ruido ("No hay datos...").
            if (sourceTable.Rows.Count == 0)
            {
                ShowMessage("El archivo no contiene filas para importar.", "Aviso", MessageBoxIcon.Warning);
                return;
            }

            var activeDefinitions = BuildActiveDefinitions();
            try
            {
                _importManager.SetMapping(activeDefinitions);
                _importManager.ReadData(sourceTable);
                // El retorno bool es la única fuente de decisión: mantener dos
                // lecturas (bool + colección de errores) invita a que diverjan.
                var isValid = _importManager.ValidateDataTypes();
                UpdateRowErrorFlags(sourceTable);

                // El proceso se notifica antes de decidir el cierre para que derivaciones
                // puedan abortar (lanzando una excepción) si su lógica lo requiere.
                OnProcessImportFinished();

                if (isValid)
                {
                    // La tabla resultado es una proyección tipada propia del asistente:
                    // exponer el DataSource interno (todo string + columnas de control)
                    // filtraría detalles de implementación y acoplaría al consumidor
                    // al ciclo de vida del ImportManager.
                    ResultData = _importManager.CreateResultTable();
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

            CommitPendingGridEdit();

            var activeDefinitions = BuildActiveDefinitions();
            try
            {
                _importManager.SetMapping(activeDefinitions);
                _importManager.ReadData(sourceTable);
                // Misma fuente única de decisión que ImportData: el retorno bool.
                var isValid = _importManager.ValidateDataTypes();
                UpdateRowErrorFlags(sourceTable);
                OnProcessImportFinished();

                if (isValid)
                {
                    // Misma proyección tipada que ImportData: el resultado de la
                    // revalidación debe ser consistente con el de la importación.
                    ResultData = _importManager.CreateResultTable();
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
        /// <remarks>
        /// Trabaja sobre clones de las definiciones originales: la colección pertenece
        /// al consumidor del wizard y puede reutilizarse en importaciones posteriores
        /// (o compartirse entre wizards), por lo que el mapeo activo es una traducción
        /// interna y nunca un efecto lateral sobre el estado del llamador.
        /// </remarks>
        private FieldDefinitionCollection BuildActiveDefinitions()
        {
            var activeDefinitions = new FieldDefinitionCollection();
            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                var originalDef = row.Tag as FieldDefinitionItem;
                string selectedSourceCol = row.Cells[1].Value?.ToString()!;

                if (originalDef is null) continue;

                var def = (FieldDefinitionItem)originalDef.Clone();
                if (selectedSourceCol != "(Ignorar)" && !string.IsNullOrEmpty(selectedSourceCol))
                {
                    def.SourceColumnName = selectedSourceCol;
                }
                else
                {
                    // Nombre ficticio: el ImportManager no encontrará la columna y el
                    // valor extraído será null (equivale a ignorar el campo).
                    def.SourceColumnName = def.DisplayName;
                }
                activeDefinitions.Add(def);
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
                        // PropertyName transporta el FieldName interno ("Age"); el usuario
                        // piensa en términos de negocio, así que se resuelve el DisplayName
                        // de la definición. Los fallos sin propiedad son mensajes generales.
                        var displayName = failure != null
                            ? _fieldDefinitions.FirstOrDefault(f => f.FieldName == failure.PropertyName)?.DisplayName ?? failure.PropertyName
                            : "General";
                        return new
                        {
                            // Convertimos a string para asegurar consistencia de tipo en la
                            // lista anónima (int vs string "-")
                            Fila = failure != null ? failure.IndexRow.ToString() : "-",
                            Campo = displayName,
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

            // Columna de control que refleja por fila el resultado de la última validación;
            // permite filtrar solo-errores con un DataView sin copiar filas (ver FilterHasErrors).
            EnsureHasErrorColumn(table);

            // Se enlaza una VISTA de la tabla y no la tabla directamente: así el filtrado
            // solo-errores se resuelve cambiando el RowFilter de la vista, y las ediciones
            // del grid viajan siempre a la misma tabla subyacente.
            dgvPreview.DataSource = new DataView(table);

            // La columna de control es infraestructura del filtro; no aporta nada al usuario.
            if (dgvPreview.Columns["_HasError"] is { } errorColumn)
                errorColumn.Visible = false;

            ResultData = null;

            PopulateMappingGrid();

            // Un origen sin filas (p. ej. CSV de solo encabezados) no tiene nada
            // que importar: se deshabilita el botón y se avisa en la barra de estado
            // en lugar de dejar que la validación genere ruido engañoso.
            var hasRows = table.Rows.Count > 0;
            btnImport.Enabled = hasRows;
            if (hasRows)
            {
                tsslWarning.Visible = false;
            }
            else
            {
                tsslWarning.Text = "El archivo no contiene filas para importar.";
                tsslWarning.Visible = true;
            }
            tsslCount.Text = $"Filas cargadas: {table.Rows.Count:N0}";
        }

        /// <summary>
        /// Ocurre cuando finaliza el proceso de importación de datos
        /// </summary>
        protected virtual void OnProcessImportFinished()
        {
            // _importManager es readonly y siempre está asignado en el ctor:
            // el null-check era ruido de un contrato ya garantizado.
            if (_importManager.ValidationErrors.Errors.Count > 0)
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

        /// <summary>
        /// Reacciona a un cambio en las opciones del control de configuración marcando
        /// los datos cargados como obsoletos: invalida el resultado, deshabilita la
        /// importación y pide al usuario recargar los datos.
        /// </summary>
        /// <param name="sender">Control de configuración que cambió (no utilizado).</param>
        /// <param name="e">Argumentos del evento (no utilizados).</param>
        /// <remarks>
        /// La recarga automática se descartó deliberadamente: pisaría las ediciones del
        /// usuario en el preview sin aviso. En su lugar se invalida el estado derivado y
        /// se guía al usuario. El guard de <c>loadedDataTable is null</c> ignora los
        /// eventos que el propio control dispara durante su inicialización (p. ej.
        /// <see cref="ExcelConfigControl"/> selecciona la primera hoja de forma asíncrona
        /// y eleva <c>OptionsChanged</c> antes de que exista información cargada).
        /// </remarks>
        protected virtual void OnConfigOptionsChanged(object? sender, EventArgs e)
        {
            if (loadedDataTable is null) return;

            // Cancelar revalidaciones diferidas pendientes: con opciones cambiadas ya no
            // tendrían sentido y restablecerían un ResultData obsoleto.
            _revalidateDebounce.Stop();

            ResultData = null;
            btnImport.Enabled = false;
            tsslWarning.Text = "La configuración de análisis cambió: recargue los datos.";
            tsslWarning.Visible = true;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ShowOpenDialogFile();
        }

        private void btnImport_Click(object sender, EventArgs e)
        { ImportData(); }

        private async void btnLoadData_Click(object sender, EventArgs e)
        {
            // Guard de reentrada: mientras una carga está en curso se ignoran clicks
            // adicionales y el botón permanece deshabilitado; la carga real es asíncrona
            // para no congelar el hilo de UI con archivos grandes.
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                btnLoadData.Enabled = false;
                await LoadDataAsync();
            }
            finally
            {
                _isLoading = false;
                btnLoadData.Enabled = true;
            }
        }

        private void cbFilterHasError_CheckedChanged(object sender, EventArgs e)
        {
            var table = loadedDataTable;
            if (table is null) return;
            FilterHasErrors(table, dgvPreview, cbFilterHasError.Checked);
        }

        /// <summary>
        /// Reprograma la revalidación silenciosa tras una edición de celda o de mapeo.
        /// </summary>
        /// <remarks>
        /// Reiniciar (Stop + Start) la ventana de 400 ms coalescea ráfagas de ediciones
        /// rápidas en una sola pasada de validación, que reconstruye las definiciones
        /// activas y valida la tabla completa.
        /// </remarks>
        protected void ScheduleSilentRevalidation()
        {
            _revalidateDebounce.Stop();
            _revalidateDebounce.Start();
        }

        private void dgvPreview_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // La edición del preview solo revalida en silencio (con debounce); la
            // decisión de cerrar corresponde exclusivamente al botón Importar.
            ScheduleSilentRevalidation();
        }

        private void dgvMapping_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Solo la columna de origen (índice 1) afecta la conversión; durante el
            // poblado programático del grid el evento es ruido, no intención de usuario.
            if (_populatingMapping || loadedDataTable is null || e.ColumnIndex != 1 || e.RowIndex < 0)
            {
                return;
            }

            ScheduleSilentRevalidation();
        }

        /// <summary>
        /// Filtra el grid de previsualización para mostrar únicamente las filas con
        /// errores de validación (o restaurar la vista completa).
        /// </summary>
        /// <remarks>
        /// El filtrado se implementa con un <see cref="DataView"/> sobre la MISMA tabla
        /// cargada. El enfoque anterior (clonar la tabla y copiar las filas con error vía
        /// ImportRow) se descartó porque la copia es independiente: las correcciones que
        /// el usuario hacía en la vista filtrada se perdían al restaurar el filtro, y el
        /// estado de errores quedaba desincronizado entre copia y original.
        /// </remarks>
        /// <param name="dt">Tabla cargada cuyas filas se muestran en el preview.</param>
        /// <param name="dgv">Grid de previsualización a filtrar.</param>
        /// <param name="showErrorOnly"><c>true</c> para mostrar solo filas con error; <c>false</c> para la vista completa.</param>
        protected virtual void FilterHasErrors(DataTable dt, DataGridView dgv, bool showErrorOnly)
        {
            EnsureHasErrorColumn(dt);
            dgv.DataSource = showErrorOnly
                ? new DataView(dt, "[_HasError] = true", string.Empty, DataViewRowState.CurrentRows)
                : new DataView(dt);
        }

        /// <summary>
        /// Garantiza que la tabla cargada contenga la columna de control
        /// <c>_HasError</c> usada por el RowFilter del preview.
        /// </summary>
        private static void EnsureHasErrorColumn(DataTable table)
        {
            if (!table.Columns.Contains("_HasError"))
                table.Columns.Add("_HasError", typeof(bool));
        }

        /// <summary>
        /// Confirma cualquier edición pendiente del grid de previsualización antes de
        /// ejecutar el pipeline de validación.
        /// </summary>
        /// <remarks>
        /// Las escrituras programáticas sobre celdas enlazadas dejan la fila subyacente
        /// en modo de edición (<see cref="DataRowView"/> con versión Proposed): la
        /// validación leería el valor propuesto, pero el <see cref="DataView"/> del
        /// preview — y por tanto el filtro solo-errores — evalúa la versión Current,
        /// de modo que la fila corregida no saldría de la vista filtrada.
        /// <see cref="CurrencyManager.EndCurrentEdit"/> es el único mecanismo que
        /// consolida esa edición pendiente (CommitEdit/EndEdit del grid no la comprometen),
        /// sincronizando el estado visible con el validado.
        /// </remarks>
        private void CommitPendingGridEdit()
        {
            if (dgvPreview.DataSource is null || dgvPreview.BindingContext is null) return;
            (dgvPreview.BindingContext[dgvPreview.DataSource] as CurrencyManager)?.EndCurrentEdit();
        }

        /// <summary>
        /// Sincroniza la columna de control <c>_HasError</c> con el estado de errores de
        /// cada fila tras una validación, de modo que el DataView del preview (y su filtro
        /// solo-errores) refleje siempre el resultado de la última validación.
        /// </summary>
        /// <param name="table">Tabla cargada cuyas banderas se actualizan.</param>
        protected virtual void UpdateRowErrorFlags(DataTable table)
        {
            EnsureHasErrorColumn(table);

            foreach (DataRow row in table.Rows)
                row["_HasError"] = row.HasErrors;
        }

        private void OnDispose()
        {
            _revalidateDebounce.Dispose();
            loadedDataTable?.Dispose();
            ResultData?.Dispose();
            if (_importManager != null)
                _importManager.Dispose();
        }

        /// <summary>
        /// Devuelve el asistente a su estado inicial: sin preview, sin mapeo,
        /// sin resultado y sin botones activos.
        /// </summary>
        /// <remarks>
        /// Se invoca cuando <see cref="FileName"/> se asigna vacío: el consumidor
        /// limpió la selección y la UI no debe conservar datos de un archivo ya
        /// descartado. Se anula la suscripción del control de configuración saliente
        /// porque el panel se vacía: retenerla sería una fuga de suscripción.
        /// </remarks>
        protected virtual void ResetWizardState()
        {
            // Sin tabla cargada, ninguna revalidación diferida tiene sentido.
            _revalidateDebounce.Stop();

            // Asignación directa del campo (no de la propiedad) para no disparar
            // OnLoadedDataSource con null: el limpiado de la UI se hace aquí explícito.
            loadedDataTable = null;

            ResultData?.Dispose();
            ResultData = null;

            dgvPreview.DataSource = null;
            dgvErrors.DataSource = null;
            dgvMapping.Rows.Clear();

            btnImport.Enabled = false;
            btnLoadData.Enabled = false;

            tsslCount.Text = "Filas cargadas: 0";
            tsslWarning.Visible = false;

            if (currentConfigControl is not null)
            {
                currentConfigControl.OptionsChanged -= OnConfigOptionsChanged;
                currentConfigControl = null;
            }
            pnlConfig.Controls.Clear();
        }

        private void OnSelectedFile()
        {
            // Lógica para detectar el tipo de archivo y cambiar el modo automáticamente
            currentExtFile = Path.GetExtension(txtFilePath.Text).ToLower();
            btnLoadData.Enabled = false;

            // Desuscribir el control saliente: SourceTypeUpdate lo descarta del panel y
            // crea uno nuevo en cada selección; sin esto el control viejo retendría el
            // handler vivo (fuga de suscripción).
            if (currentConfigControl is not null)
                currentConfigControl.OptionsChanged -= OnConfigOptionsChanged;

            var configControl = currentExtFile == ".xlsx" || currentExtFile == ".xls"
                ? SourceTypeUpdate("Excel")
                : SourceTypeUpdate("CSV");
            currentConfigControl = configControl;

            // Ahora inicializamos el control con la ruta del archivo (esto puede preconfigurar delimitadores)
            configControl.Initialize(txtFilePath.Text);

            // Suscribir DESPUÉS de Initialize: los controles pueden disparar OptionsChanged
            // durante su inicialización (p. ej. Excel al seleccionar la primera hoja); el
            // handler lo ignora mientras no existan datos cargados.
            configControl.OptionsChanged += OnConfigOptionsChanged;
            btnLoadData.Enabled = true;
        }

        private void PopulateMappingGrid()
        {
            var table = loadedDataTable;
            if (table is null) return;

            // El poblado programático dispara CellValueChanged por cada asignación:
            // se silencia el handler para no programar revalidaciones espurias.
            _populatingMapping = true;
            try
            {
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
            finally
            {
                _populatingMapping = false;
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