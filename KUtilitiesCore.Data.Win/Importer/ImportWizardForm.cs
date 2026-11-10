using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Data.Wizard;
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
    /// <remarks>
    /// <para>
    /// El estado lógico del asistente (tabla cargada, carga en curso, obsolescencia,
    /// debounce de revalidación y resultado tipado) vive en el orquestador compartido
    /// <see cref="ImportWizardController"/> de KUtilitiesCore.Data.Wizard; este formulario
    /// conserva exclusivamente la interacción de vista: pintado de grids, diálogos,
    /// mensajes, el commit de ediciones pendientes y el hosting de paneles de
    /// configuración.
    /// </para>
    /// <para>
    /// El repintado de errores y del aviso de la barra de estado se centraliza en el
    /// manejo de <see cref="ImportWizardController.ErrorsChanged"/>, de modo que tanto las
    /// revalidaciones explícitas (importar, editar) como las diferidas por debounce
    /// producen el mismo efecto visual.
    /// </para>
    /// </remarks>
    public partial class ImportWizardForm : Form
    {
        #region Fields

        private readonly FieldDefinitionCollection _fieldDefinitions;

        /// <summary>
        /// Referencia de solo lectura al gestor compartido con el orquestador: el
        /// formulario la usa para pintar el grid de errores y el conteo de la
        /// advertencia; su ciclo de vida pertenece al controlador, que lo dispone
        /// en <see cref="ImportWizardController.Dispose"/>.
        /// </summary>
        private readonly ImportManager _importManager;

        /// <summary>Orquestador compartido dueño del estado lógico del asistente.</summary>
        private readonly ImportWizardController _controller;

        // Estado de vista
        private IImportConfigControl? currentConfigControl;

        private string? currentExtFile;

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
            // El estado no-UI se inicializa ANTES de InitializeComponent: el Designer dispara
            // eventos de grid (p. ej. CellValueChanged al asignar HeaderText) que ya consultan
            // el estado lógico a través del orquestador.
            _fieldDefinitions = fieldDefinitions ?? [];
            _importManager = importManager ?? new ImportManager();

            // El commit de ediciones pendientes debe ocurrir ANTES de que el orquestador
            // revalide al dispararse el debounce: este handler se suscribe primero y el
            // del orquestador (en su constructor) corre después en el mismo evento.
            var debounceTimer = new WinFormsDebounceTimer();
            debounceTimer.Elapsed += OnDebounceCommitPendingEdit;

            _controller = new ImportWizardController(_fieldDefinitions, _importManager, debounceTimer)
            {
                // El mapeo activo lo decide el usuario en el grid de mapeo: se resuelve en
                // el momento exacto de validar (pull), porque el grid cambia sin notificar
                // al orquestador.
                ActiveDefinitionsResolver = BuildActiveDefinitions
            };
            _controller.LoadCompleted += OnControllerLoadCompleted;
            _controller.LoadFailed += OnControllerLoadFailed;
            _controller.ErrorsChanged += OnControllerErrorsChanged;

            InitializeComponent();
            dgvMapping.AutoGenerateColumns = false;
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
                // Sincroniza el estado del orquestador: vacío dispara su propio reset.
                _controller.FileName = value ?? string.Empty;
                if (string.IsNullOrEmpty(value))
                    ResetWizardState();
                else
                    OnSelectedFile();
            }
        }

        /// <summary>
        /// Datos cargados de la fuente de datos.
        /// </summary>
        /// <remarks>
        /// <para>
        /// La asignación adopta la tabla a través del orquestador: se enlaza una vista
        /// sobre la MISMA tabla (sin clonar) y los errores de validación se pintan
        /// directamente sobre ella. La tabla sigue siendo propiedad del llamador: el
        /// orquestador no la dispone, la libera este formulario al cerrarse.
        /// </para>
        /// <para>
        /// Asignar <see langword="null"/> se ignora: sin tabla no hay nada que adoptar ni
        /// pintar, y conservar la carga anterior es más seguro que dejar la vista apuntando
        /// a datos que ya no existen.
        /// </para>
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Data.DataTable? LoadedDataTable
        {
            get => _controller.LoadedDataView?.Table;
            protected set
            {
                if (value is null) return;
                _controller.Load(new TableDataSourceReader(value));
            }
        }

        /// <summary>
        /// Datos requeridos para la importación
        /// </summary>
        /// <remarks>
        /// Proyección tipada materializada por el orquestador tras una validación exitosa;
        /// es legible incluso después de disponer el asistente.
        /// </remarks>
        public System.Data.DataTable? ResultData => _controller.ResultData;

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
                _controller.Load(reader);
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
        /// Las continuaciones se ejecutan en el contexto de UI porque el orquestador no usa
        /// <c>ConfigureAwait(false)</c>: al completarse toca controles de UI (preview, mapeo,
        /// barra de estado) que exigen el hilo de UI. Reutiliza <see cref="CreateReader"/> y
        /// <see cref="HandleLoadError"/> (vía <see cref="ImportWizardController.LoadFailed"/>)
        /// para mantener una sola ruta de decisiones, y el lector se dispone en el bloque
        /// <c>finally</c> por la misma razón que en la carga síncrona: los lectores de Excel
        /// mantienen el archivo físico abierto hasta su disposición.
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
                await _controller.LoadAsync(reader);
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
        /// Se invoca desde el evento <see cref="ImportWizardController.LoadFailed"/> del
        /// orquestador, que ya capturó la excepción del lector.
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
        /// <see cref="AutoCloseOnSuccess"/>); con errores permanece abierto para que el
        /// usuario corrija los datos.
        /// </summary>
        /// <remarks>
        /// El pipeline de validación (mapeo activo, relectura de la tabla adoptada,
        /// validación de tipos y banderas de error) pertenece al orquestador
        /// (<see cref="ImportWizardController.Import()"/>); este método conserva las
        /// decisiones de vista: mensajes, cambio de pestaña y cierre.
        /// </remarks>
        protected virtual void ImportData()
        {
            var sourceTable = LoadedDataTable;
            if (sourceTable is null)
            {
                ShowMessage("No hay datos cargados para importar.", "Aviso", MessageBoxIcon.Warning);
                return;
            }

            CommitPendingGridEdit();

            // Un origen vacío no es un error de validación: se avisa al usuario y
            // se sale sin llenar la pestaña de errores con ruido ("No hay datos...").
            if (sourceTable.Rows.Count == 0)
            {
                ShowMessage("El archivo no contiene filas para importar.", "Aviso", MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // La importación explícita cancela el debounce pendiente dentro del propio
                // orquestador (Revalidate detiene el temporizador).
                var args = _controller.Import();

                // El proceso se notifica antes de decidir el cierre para que derivaciones
                // puedan abortar (lanzando una excepción) si su lógica lo requiere.
                OnProcessImportFinished();

                if (args.IsValid)
                {
                    // La tabla resultado (proyección tipada propia del asistente, sin
                    // columnas de control) ya fue materializada por el orquestador.
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
                    ShowMessage(
                        $"Se encontraron {args.ErrorCount} errores de validación.",
                        "Errores",
                        MessageBoxIcon.Warning);
                    tsslWarning.Text = "Los datos contienen errores, favor de verificar.";
                    tsslWarning.Visible = true;
                    tabControlResults.SelectedTab = tabControlResults.TabPages[1];

                    // No cerramos el form para permitir correcciones
                    DialogResult = DialogResult.None;
                }
            }
            catch (Exception ex)
            {
                // Un proceso abortado por una derivación no deja resultado utilizable,
                // igual que si la validación hubiera fallado.
                _controller.DiscardResult();

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
        /// <remarks>
        /// El pipeline pertenece al orquestador (<see cref="ImportWizardController.Revalidate()"/>);
        /// el repintado (grid de errores y aviso) lo produce el evento
        /// <see cref="ImportWizardController.ErrorsChanged"/>, compartido con las
        /// revalidaciones diferidas por debounce.
        /// </remarks>
        protected virtual void RevalidateAfterEdit()
        {
            var sourceTable = LoadedDataTable;
            if (sourceTable is null) return;

            CommitPendingGridEdit();

            try
            {
                _controller.Revalidate();
                OnProcessImportFinished();
            }
            catch
            {
                // La revalidación por edición es de fondo: solo deja el aviso persistente;
                // el diagnóstico detallado se reserva para el botón Importar.
                _controller.DiscardResult();
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
        /// interna y nunca un efecto lateral sobre el estado del llamador. Es el
        /// origen del <see cref="ImportWizardController.ActiveDefinitionsResolver"/>
        /// del orquestador.
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
        /// Repinta la vista de errores tras cualquier cambio en la exposición neutral del
        /// orquestador: grid de errores y aviso de la barra de estado.
        /// </summary>
        /// <remarks>
        /// Centraliza el repintado de las revalidaciones explícitas y las diferidas por
        /// debounce en un solo punto: el aviso refleja el resultado de la última validación
        /// (resultado materializado = válido). Sin tabla cargada (reset) no toca los
        /// indicadores: la limpieza de vista la hace <see cref="ResetWizardState"/>.
        /// </remarks>
        private void OnControllerErrorsChanged(object? sender, EventArgs e)
        {
            RefreshErrorsGrid();

            if (_controller.LoadedDataView?.Table is null) return;

            if (_controller.ResultData is not null)
            {
                tsslWarning.Visible = false;
            }
            else
            {
                tsslWarning.Text = "Los datos contienen errores, favor de verificar.";
                tsslWarning.Visible = true;
            }
        }

        /// <summary>
        /// Repinta el pipeline de vista tras una carga exitosa del orquestador: preview,
        /// grid de mapeo, botones y barra de estado.
        /// </summary>
        private void OnControllerLoadCompleted(object? sender, EventArgs e)
        {
            var table = _controller.LoadedDataView?.Table;
            if (table is null) return;

            // La columna de control y las banderas por fila ya fueron garantizadas por el
            // orquestador al adoptar la tabla; se enlaza la vista que él expone para que
            // el filtrado solo-errores y las ediciones operen siempre sobre la misma tabla.
            dgvPreview.DataSource = _controller.LoadedDataView;

            // La columna de control es infraestructura del filtro; no aporta nada al usuario.
            if (dgvPreview.Columns["_HasError"] is { } errorColumn)
                errorColumn.Visible = false;

            PopulateMappingGrid();

            // Un origen sin filas (p. ej. CSV de solo encabezados) no tiene nada
            // que importar: se deshabilita el botón y se avisa en la barra de estado
            // en lugar de dejar que la validación genere ruido engañoso.
            btnImport.Enabled = _controller.CanImport;
            if (table.Rows.Count > 0)
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
        /// Canaliza el fallo de carga del orquestador hacia el manejo de vista existente:
        /// la excepción original viaja en los argumentos neutrales del evento.
        /// </summary>
        private void OnControllerLoadFailed(object? sender, LoadFailedEventArgs e)
        {
            HandleLoadError(e.Exception);
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
        /// se guía al usuario. El guard de <c>LoadedDataTable is null</c> ignora los
        /// eventos que el propio control dispara durante su inicialización (p. ej.
        /// <see cref="ExcelConfigControl"/> selecciona la primera hoja de forma asíncrona
        /// y eleva <c>OptionsChanged</c> antes de que exista información cargada).
        /// </remarks>
        protected virtual void OnConfigOptionsChanged(object? sender, EventArgs e)
        {
            if (LoadedDataTable is null) return;

            // Cancela revalidaciones diferidas y retira el resultado: con opciones
            // cambiadas ya no tendrían sentido y restablecerían un resultado obsoleto.
            _controller.InvalidateData();

            btnImport.Enabled = false;
            tsslWarning.Text = "La configuración de análisis cambió: recargue los datos.";
            tsslWarning.Visible = true;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ShowOpenDialogFile();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ImportData();
        }

        private async void btnLoadData_Click(object sender, EventArgs e)
        {
            // Guard de reentrada: mientras una carga está en curso se ignoran clicks
            // adicionales y el botón permanece deshabilitado; la carga real es asíncrona
            // para no congelar el hilo de UI con archivos grandes.
            if (_controller.IsLoading) return;
            try
            {
                btnLoadData.Enabled = false;
                await LoadDataAsync();
            }
            finally
            {
                btnLoadData.Enabled = true;
            }
        }

        private void cbFilterHasError_CheckedChanged(object sender, EventArgs e)
        {
            var table = LoadedDataTable;
            if (table is null) return;
            FilterHasErrors(table, dgvPreview, cbFilterHasError.Checked);
        }

        /// <summary>
        /// Reprograma la revalidación silenciosa tras una edición de celda o de mapeo.
        /// </summary>
        /// <remarks>
        /// La ventana de estabilización pertenece al orquestador: reiniciarla con cada
        /// invocación coalescea ráfagas de ediciones rápidas en una sola pasada de
        /// validación, que reconstruye las definiciones activas y valida la tabla completa.
        /// </remarks>
        protected void ScheduleSilentRevalidation()
        {
            _controller.ScheduleSilentRevalidation();
        }

        /// <summary>
        /// Consolida las ediciones pendientes del grid justo antes de que el orquestador
        /// revalide al dispararse el debounce.
        /// </summary>
        /// <remarks>
        /// Este handler se suscribe al temporizador ANTES que el del orquestador, por lo
        /// que en cada disparo el commit corre primero y la revalidación lee la tabla con
        /// los valores ya consolidados.
        /// </remarks>
        private void OnDebounceCommitPendingEdit(object? sender, EventArgs e)
        {
            CommitPendingGridEdit();
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
            if (_populatingMapping || LoadedDataTable is null || e.ColumnIndex != 1 || e.RowIndex < 0)
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
        /// estado de errores quedaba desincronizado entre copia y original. La semántica
        /// del filtro (columna de control + RowFilter) la centraliza el orquestador en
        /// <see cref="ImportWizardController.CreateErrorsOnlyView(DataTable)"/>.
        /// </remarks>
        /// <param name="dt">Tabla cargada cuyas filas se muestran en el preview.</param>
        /// <param name="dgv">Grid de previsualización a filtrar.</param>
        /// <param name="showErrorOnly"><c>true</c> para mostrar solo filas con error; <c>false</c> para la vista completa.</param>
        protected virtual void FilterHasErrors(DataTable dt, DataGridView dgv, bool showErrorOnly)
        {
            dgv.DataSource = showErrorOnly
                ? ImportWizardController.CreateErrorsOnlyView(dt)
                : new DataView(dt);
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

        private void OnDispose()
        {
            // La tabla cargada es propiedad del formulario: el orquestador la adopta sin
            // disponerla (contrato de la capa compartida), así que su liberación sigue aquí.
            _controller.LoadedDataView?.Table?.Dispose();

            // El orquestador libera el temporizador de debounce, el gestor de importación
            // y el resultado tipado.
            _controller.Dispose();
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
            // Reset del estado lógico: detiene el debounce, retira la vista cargada
            // (sin disponer la tabla del llamador), descarta el resultado y limpia
            // la exposición de errores.
            _controller.Reset();

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
            var table = LoadedDataTable;
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

        /// <summary>
        /// Adaptador que presenta una tabla ya leída como un
        /// <see cref="IDataSourceReader"/>: el orquestador solo acepta cargas vía lector,
        /// y este tipo permite inyectar tablas existentes (p. ej. desde la propiedad
        /// <see cref="LoadedDataTable"/>) sin tocar el sistema de archivos.
        /// </summary>
        private sealed class TableDataSourceReader : IDataSourceReader
        {
            private readonly DataTable _table;

            /// <summary>Inicializa el adaptador con la tabla que devolverá el lector.</summary>
            public TableDataSourceReader(DataTable table)
            {
                _table = table;
            }

            /// <summary>Siempre se puede leer: la tabla ya está en memoria.</summary>
            public bool CanRead => true;

            /// <summary>Devuelve la tabla adaptada sin copiarla.</summary>
            public DataTable ReadData() => _table;

            /// <summary>Devuelve la tabla adaptada sin copiarla, ya resuelta.</summary>
            public Task<DataTable> ReadDataAsync() => Task.FromResult(_table);
        }

        #endregion Methods
    }
}
