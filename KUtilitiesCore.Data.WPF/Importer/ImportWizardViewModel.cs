using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Wizard;
using KUtilitiesCore.Data.Wizard.Configuration;
using KUtilitiesCore.MVVM;
using KUtilitiesCore.MVVM.Command;
using KUtilitiesCore.MVVM.MessageService;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;

namespace KUtilitiesCore.Data.WPF.Importer
{
    /// <summary>
    /// ViewModel del asistente de importación WPF: expone el estado y las operaciones del
    /// orquestador compartido (<see cref="ImportWizardController"/>) hacia el enlace de
    /// datos de WPF, traduciendo sus eventos neutrales a notificaciones
    /// <see cref="INotifyPropertyChanged"/> y comandos ejecutables.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contrato de afinidad de hilo: todo el estado se manipula desde el hilo de interfaz
    /// del consumidor; la carga asíncrona no usa <c>ConfigureAwait(false)</c> para que la
    /// continuación regrese al contexto del llamador (los <see cref="DataTable"/>/
    /// <see cref="DataView"/> subyacentes exigen afinidad de un solo hilo).
    /// </para>
    /// <para>
    /// Los mensajes al usuario se enrutan por una referencia propia de
    /// <see cref="ISupportMessageService"/> (no la heredada de la base, cuyo setter es
    /// <c>internal</c> y por tanto no es inyectable desde este ensamblado).
    /// </para>
    /// <example>
    /// <code>
    /// var viewModel = new ImportWizardViewModel(definiciones, messageService: new WpfMessageService());
    /// viewModel.OpenFileRequested += (s, e) => { /* host: OpenFileDialog y asignar FileName */ };
    /// viewModel.CloseRequested += (s, e) => ventana.Close();
    /// </code>
    /// </example>
    /// </remarks>
    public class ImportWizardViewModel : ViewModelHelperBase, IDisposable
    {
        private readonly ImportWizardController _controller;
        private readonly FieldDefinitionCollection _fieldDefinitions;
        private readonly ISupportMessageService? _messageService;
        private string _fileName = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _autoCloseOnSuccess = true;
        private bool _showErrorsOnly;
        private IImportConfigControl? _currentConfig;
        private int _loadedRowCount;
        private bool _disposed;

        /// <summary>
        /// Inicializa el ViewModel creando el orquestador subyacente con las definiciones
        /// de campo indicadas y suscribiéndose a sus eventos neutrales.
        /// </summary>
        /// <param name="fieldDefinitions">
        /// Definiciones de campos destino que gobiernan la validación y el resultado tipado.
        /// </param>
        /// <param name="importManager">
        /// Gestor de importación existente; si es <see langword="null"/>, el orquestador
        /// crea uno propio y toma su propiedad.
        /// </param>
        /// <param name="debounceTimer">
        /// Temporizador de revalidación diferida; si es <see langword="null"/>, las
        /// revalidaciones silenciosas programadas no se ejecutan (front-end mínimo).
        /// </param>
        /// <param name="messageService">
        /// Servicio de mensajes al usuario; si es <see langword="null"/>, los avisos se
        /// omiten de forma silenciosa (útil en pruebas).
        /// </param>
        /// <exception cref="ArgumentNullException">Si <paramref name="fieldDefinitions"/> es <see langword="null"/>.</exception>
        public ImportWizardViewModel(FieldDefinitionCollection fieldDefinitions,
            ImportManager? importManager = null,
            IWizardDebounceTimer? debounceTimer = null,
            ISupportMessageService? messageService = null)
        {
            ArgumentNullException.ThrowIfNull(fieldDefinitions);
            _fieldDefinitions = fieldDefinitions;
            _messageService = messageService;
            _controller = new ImportWizardController(fieldDefinitions, importManager, debounceTimer)
            {
                ActiveDefinitionsResolver = BuildActiveDefinitions
            };
            _controller.LoadCompleted += OnControllerLoadCompleted;
            _controller.LoadFailed += OnControllerLoadFailed;
            _controller.ErrorsChanged += OnControllerErrorsChanged;
            _controller.ImportCompleted += OnControllerImportCompleted;

            BrowseFileCommand = RelayCommand<ImportWizardViewModel>.Create(this, vm => vm.BrowseFile(), "BrowseFile");
            LoadDataCommand = RelayCommand<ImportWizardViewModel>.Create(this, vm => vm.LoadData(), "LoadData");
            ImportCommand = RelayCommand<ImportWizardViewModel>.Create(this, vm => vm.ImportData(), "Import");
        }

        /// <summary>
        /// Solicita al host que muestre el diálogo de selección de archivo; el host debe
        /// asignar el resultado a <see cref="FileName"/>.
        /// </summary>
        public event EventHandler? OpenFileRequested;

        /// <summary>
        /// Solicita al host que cierre la ventana del asistente (vía
        /// <see cref="AutoCloseOnSuccess"/> tras una importación exitosa).
        /// </summary>
        public event EventHandler? CloseRequested;

        /// <summary>
        /// Orquestador subyacente, expuesto para escenarios avanzados de hosting
        /// (por ejemplo, construir vistas filtradas de errores).
        /// </summary>
        public ImportWizardController Controller => _controller;

        /// <summary>
        /// Comando que abre el diálogo de selección de archivo.
        /// </summary>
        public RelayCommand<ImportWizardViewModel> BrowseFileCommand { get; }

        /// <summary>
        /// Comando que carga los datos del archivo con la configuración actual.
        /// </summary>
        public RelayCommand<ImportWizardViewModel> LoadDataCommand { get; }

        /// <summary>
        /// Comando que valida e importa los datos cargados.
        /// </summary>
        public RelayCommand<ImportWizardViewModel> ImportCommand { get; }

        /// <summary>
        /// Indica si la ventana debe cerrarse automáticamente tras una importación
        /// exitosa (comportamiento por defecto).
        /// </summary>
        public bool AutoCloseOnSuccess
        {
            get => _autoCloseOnSuccess;
            set => this.SetVMValue(ref _autoCloseOnSuccess, value);
        }

        /// <summary>
        /// Ruta del archivo a importar; asignarla reconstruye la configuración de análisis
        /// apropiada para el tipo de archivo (CSV vs Excel). Una cadena vacía limpia el
        /// estado del asistente.
        /// </summary>
#pragma warning disable CS8601 // La firma de SetVMValue usa TProperty? aunque el campo no admita nulos; paridad con Title en ViewModelHelperBase.
        public string FileName
        {
            get => _fileName;
            set
            {
                if (this.SetVMValue(ref _fileName, value ?? string.Empty))
                    OnFileNameChanged();
            }
        }
#pragma warning restore CS8601

        /// <summary>
        /// Configuración de análisis activa según el tipo de archivo seleccionado.
        /// </summary>
        public IImportConfigControl? CurrentConfig => _currentConfig;

        /// <summary>
        /// Vista sobre los datos cargados para el preview; <see langword="null"/> si no hay datos.
        /// </summary>
        public DataView? LoadedDataView => _controller.LoadedDataView;

        /// <summary>
        /// Resultado tipado de la última validación exitosa; <see langword="null"/> si la
        /// validación falló o aún no se ejecutó. El orquestador es dueño de su ciclo de vida.
        /// </summary>
        public DataTable? ResultData => _controller.ResultData;

        /// <summary>
        /// Indica si la configuración cambió tras la última carga y los datos deben recargarse.
        /// </summary>
        public bool IsDataStale => _controller.IsDataStale;

        /// <summary>
        /// Errores por fila expuestos de forma neutral tras la última validación.
        /// </summary>
        public IReadOnlyList<WizardRowError> RowErrors => _controller.RowErrors;

        /// <summary>
        /// Determina si la importación puede ejecutarse: hay datos cargados, no están
        /// obsoletos y la última validación no produjo errores. A diferencia del asistente
        /// WinForms (donde el botón se habilita con datos vigentes y el usuario descubre
        /// los errores al importar), el botón WPF se deshabilita cuando hay errores presentes.
        /// </summary>
        public bool CanImport => _controller.CanImport && _controller.RowErrors.Count == 0;

        /// <summary>
        /// Número de filas cargadas del archivo.
        /// </summary>
        public int LoadedRowCount
        {
            get => _loadedRowCount;
            private set => this.SetVMValue(ref _loadedRowCount, value);
        }

        /// <summary>
        /// Mensaje de estado/aviso mostrado en la barra inferior de la ventana.
        /// </summary>
#pragma warning disable CS8601 // La firma de SetVMValue usa TProperty? aunque el campo no admita nulos; paridad con Title en ViewModelHelperBase.
        public string StatusMessage
        {
            get => _statusMessage;
            private set => this.SetVMValue(ref _statusMessage, value);
        }
#pragma warning restore CS8601

        /// <summary>
        /// Indica si la vista previa debe mostrar únicamente las filas con errores
        /// (la ventana aplica el filtro <c>[_HasError] = true</c> sobre los datos cargados).
        /// </summary>
        public bool ShowErrorsOnly
        {
            get => _showErrorsOnly;
            set => this.SetVMValue(ref _showErrorsOnly, value);
        }

        /// <summary>
        /// Filas del grid de mapeo campo destino → columna origen.
        /// </summary>
        public ObservableCollection<FieldMappingItem> Mapping { get; } = [];

        /// <inheritdoc/>
        public override void OnLoaded()
        {
        }

        /// <inheritdoc/>
        public override void OnDestroy() => Dispose();

        /// <inheritdoc/>
        protected override void OnClose(CancelEventArgs e)
        {
        }

        /// <inheritdoc/>
        protected override void UpdateCommands()
        {
        }

        /// <summary>
        /// Solicita al host la apertura del diálogo de selección de archivo.
        /// </summary>
        private void BrowseFile() => OpenFileRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Determina si el comando de selección de archivo puede ejecutarse.
        /// Es público para que la resolución reflexiva de CanExecute de
        /// <see cref="RelayCommand{TViewModel}"/> lo encuentre también en subclases de prueba.
        /// </summary>
        /// <returns>Siempre <see langword="true"/>: el diálogo de archivo siempre está disponible.</returns>
        public bool CanBrowseFile() => true;

        /// <summary>
        /// Ejecuta la carga de datos en modo fire-and-forget para el comando.
        /// </summary>
        private void LoadData() => _ = LoadDataAsync();

        /// <summary>
        /// Determina si el comando de carga puede ejecutarse.
        /// Es público para que la resolución reflexiva de CanExecute de
        /// <see cref="RelayCommand{TViewModel}"/> lo encuentre también en subclases de prueba.
        /// </summary>
        /// <returns><see langword="true"/> con un archivo seleccionado y sin una carga en curso.</returns>
        public bool CanLoadData() => !_controller.IsLoading && !string.IsNullOrEmpty(FileName);

        /// <summary>
        /// Carga los datos del archivo aplicando la configuración activa, sin bloquear la
        /// interfaz. Los errores de lectura se presentan por el servicio de mensajes.
        /// </summary>
        /// <remarks>
        /// No se usa <c>ConfigureAwait(false)</c>: la continuación debe regresar al hilo
        /// del llamador porque los objetos <see cref="DataTable"/>/<see cref="DataView"/>
        /// exigen afinidad de un solo hilo.
        /// </remarks>
        public async Task LoadDataAsync()
        {
            if (_controller.IsLoading)
                return;

            IsLoading = true;
            IDataSourceReader? reader = null;
            try
            {
                reader = CreateReader();
                if (reader is null)
                    return;

                await _controller.LoadAsync(reader);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al leer el archivo.";
                NotifyCriticalError("Error de lectura", "Error al leer el archivo.", ex);
            }
            finally
            {
                IsLoading = false;
                (reader as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        /// Valida e importa los datos cargados con el mapeo actual; en caso de éxito y con
        /// <see cref="AutoCloseOnSuccess"/> activo, solicita el cierre de la ventana.
        /// </summary>
        public void ImportData()
        {
            var table = _controller.LoadedDataView?.Table;
            if (table is null)
            {
                _ = _messageService?.ShowWarningAsync("No hay datos cargados para importar.");
                return;
            }

            if (table.Rows.Count == 0)
            {
                _ = _messageService?.ShowWarningAsync("El archivo no contiene filas para importar.");
                return;
            }

            try
            {
                var args = _controller.Import();
                if (args.IsValid)
                {
                    StatusMessage = string.Empty;
                    if (AutoCloseOnSuccess)
                        CloseRequested?.Invoke(this, EventArgs.Empty);
                    else
                        _ = _messageService?.ShowSuccessAsync("Importación completada y validada correctamente.");
                }
                else
                {
                    _ = _messageService?.ShowWarningAsync($"Se encontraron {args.ErrorCount} errores de validación.");
                    StatusMessage = "Los datos contienen errores, favor de verificar.";
                }
            }
            catch (Exception ex)
            {
                _controller.DiscardResult();
                NotifyCriticalError("Error Crítico", "Error crítico en el proceso de importación", ex);
                StatusMessage = "Error crítico en la importación, favor de verificar.";
            }
        }

        /// <summary>
        /// Determina si el comando de importación puede ejecutarse.
        /// Es público para que la resolución reflexiva de CanExecute de
        /// <see cref="RelayCommand{TViewModel}"/> lo encuentre también en subclases de prueba
        /// (la búsqueda reflexiva en un tipo derivado no alcanza métodos privados de la base).
        /// </summary>
        /// <returns><see langword="true"/> cuando <see cref="CanImport"/> lo permite.</returns>
        public bool CanImportData() => CanImport;

        /// <summary>
        /// Revalida silenciosamente los datos cargados con el mapeo actual, sin diálogos.
        /// La vista es responsable de confirmar la edición pendiente del grid antes de
        /// invocar este método (paridad con <c>CommitPendingGridEdit</c> del asistente WinForms).
        /// </summary>
        public void Revalidate()
        {
            if (_controller.LoadedDataView is null)
                return;

            try
            {
                _controller.Revalidate();
            }
            catch (Exception ex)
            {
                _controller.DiscardResult();
                NotifyCriticalError("Error Crítico", "Error crítico en el proceso de importación", ex);
                StatusMessage = "Error crítico en la importación, favor de verificar.";
            }
        }

        /// <summary>
        /// Programa una revalidación silenciosa tras la ventana de estabilización del
        /// temporizador de debounce; las ediciones sucesivas se coalescen en un solo
        /// disparo. La vista la invoca al terminar la edición de una celda de la vista
        /// previa (paridad con <c>dgvPreview_CellEndEdit</c> del asistente WinForms);
        /// el propio temporizador de la vista confirma la edición pendiente antes de
        /// que el orquestador revalide.
        /// </summary>
        public void ScheduleSilentRevalidation() => _controller.ScheduleSilentRevalidation();

        /// <summary>
        /// Crea el lector de datos apropiado para el archivo seleccionado con la
        /// configuración activa. Punto de extensión para pruebas: los tests sustituyen
        /// este método por un lector falso.
        /// </summary>
        /// <returns>
        /// Lector listo para consumirse por el orquestador; <see langword="null"/> si no
        /// hay archivo o configuración disponibles (el usuario ya fue notificado).
        /// </returns>
        protected virtual IDataSourceReader? CreateReader()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                _ = _messageService?.ShowWarningAsync("Seleccione un archivo primero.");
                return null;
            }

            if (_currentConfig is null)
            {
                _ = _messageService?.ShowWarningAsync("No hay configuración de importación disponible.");
                return null;
            }

            var options = _currentConfig.GetParsingOptions();
            var extension = Path.GetExtension(FileName).ToLowerInvariant();
            return extension is ".xlsx" or ".xls"
                ? ExcelSourceReaderFactory.CreateWithOptions(FileName, (ExcelParsingOptions)options, new ClosedXmlWorkbookReaderFactory())
                : CsvSourceReaderFactory.CreateWithOptions(FileName, (TextFileParsingOptions)options);
        }

        /// <summary>
        /// Libera los recursos del ViewModel: desuscribe los eventos del orquestador y lo
        /// dispone (temporizador, gestor de importación y resultado tipado). La tabla
        /// cargada permanece viva porque es propiedad del llamador.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            DetachCurrentConfig();
            _controller.LoadCompleted -= OnControllerLoadCompleted;
            _controller.LoadFailed -= OnControllerLoadFailed;
            _controller.ErrorsChanged -= OnControllerErrorsChanged;
            _controller.ImportCompleted -= OnControllerImportCompleted;
            _controller.Dispose();
        }

        /// <summary>
        /// Construye la configuración de análisis para el archivo recién seleccionado,
        /// reemplazando la anterior, y la inicializa antes de suscribirse a sus cambios
        /// (para ignorar los eventos de restauración, igual que el asistente WinForms).
        /// </summary>
        private void OnFileNameChanged()
        {
            _controller.FileName = _fileName;
            DetachCurrentConfig();

            if (string.IsNullOrEmpty(_fileName))
            {
                _currentConfig = null;
                RaisePropertyChanged(nameof(CurrentConfig));
                RebuildMapping();
                LoadedRowCount = 0;
                StatusMessage = string.Empty;
                return;
            }

            var extension = Path.GetExtension(_fileName).ToLowerInvariant();
            ConfigViewModelBase config = extension is ".xlsx" or ".xls"
                ? new ExcelConfigViewModel()
                : new CsvConfigViewModel();
            config.Initialize(_fileName);
            config.OptionsChanged += OnConfigOptionsChanged;
            _currentConfig = config;
            RaisePropertyChanged(nameof(CurrentConfig));
        }

        /// <summary>
        /// Desuscribe el manejo de cambios de la configuración activa antes de descartarla.
        /// </summary>
        private void DetachCurrentConfig()
        {
            if (_currentConfig is not null)
                _currentConfig.OptionsChanged -= OnConfigOptionsChanged;
        }

        /// <summary>
        /// Invalida los datos cargados cuando el usuario cambia una opción de análisis:
        /// los datos vigentes ya no corresponden a la configuración y debe recargarse.
        /// Los eventos de restauración de la inicialización no llegan aquí (el host se
        /// suscribe después de <see cref="IImportConfigControl.Initialize"/>).
        /// </summary>
        private void OnConfigOptionsChanged(object? sender, EventArgs e)
        {
            if (_controller.LoadedDataView is null)
                return;

            _controller.InvalidateData();
            StatusMessage = "La configuración de análisis cambió: recargue los datos.";
            RaisePropertyChanged(nameof(IsDataStale));
            RaisePropertyChanged(nameof(CanImport));
        }

        /// <summary>
        /// Proyecta el estado de carga del orquestado hacia las propiedades de la interfaz
        /// y reconstruye el grid de mapeo con las columnas del archivo.
        /// </summary>
        private void OnControllerLoadCompleted(object? sender, EventArgs e)
        {
            var table = _controller.LoadedDataView?.Table;
            LoadedRowCount = table?.Rows.Count ?? 0;
            StatusMessage = LoadedRowCount == 0
                ? "El archivo no contiene filas para importar."
                : string.Empty;
            RebuildMapping();
            RaisePropertyChanged(nameof(LoadedDataView));
            RaisePropertyChanged(nameof(ResultData));
            RaisePropertyChanged(nameof(RowErrors));
            RaisePropertyChanged(nameof(IsDataStale));
            RaisePropertyChanged(nameof(CanImport));
        }

        /// <summary>
        /// Presenta el error de lectura capturado por el orquestador (detalles técnicos
        /// en la sección expandible del diálogo de mensajes).
        /// </summary>
        private void OnControllerLoadFailed(object? sender, LoadFailedEventArgs e)
        {
            StatusMessage = "Error al leer el archivo.";
            NotifyCriticalError("Error de lectura", "Error al leer el archivo.", e.Exception);
            RaisePropertyChanged(nameof(CanImport));
        }

        /// <summary>
        /// Actualiza las propiedades dependientes del veredicto de validación expuesto por
        /// el orquestador; al dispararse, <see cref="RowErrors"/> y <see cref="ResultData"/>
        /// ya reflejan el veredicto completado.
        /// </summary>
        private void OnControllerErrorsChanged(object? sender, EventArgs e)
        {
            if (_controller.LoadedDataView is null)
                return;

            StatusMessage = _controller.ResultData is not null
                ? string.Empty
                : "Los datos contienen errores, favor de verificar.";
            RaisePropertyChanged(nameof(RowErrors));
            RaisePropertyChanged(nameof(ResultData));
            RaisePropertyChanged(nameof(CanImport));
        }

        /// <summary>
        /// Actualiza las propiedades dependientes tras completar una importación explícita.
        /// </summary>
        private void OnControllerImportCompleted(object? sender, ImportCompletedEventArgs e)
        {
            RaisePropertyChanged(nameof(RowErrors));
            RaisePropertyChanged(nameof(ResultData));
            RaisePropertyChanged(nameof(CanImport));
        }

        /// <summary>
        /// Reemplaza el contenido del grid de mapeo con un fila por campo definido,
        /// pre-mapeando por coincencia de nombre (ignorando mayúsculas) con las columnas
        /// del archivo; la columna centinela <c>_HasError</c> del orquestador se excluye
        /// de las columnas origen ofrecidas.
        /// </summary>
        private void RebuildMapping()
        {
            Mapping.Clear();
            var table = _controller.LoadedDataView?.Table;
            if (table is null)
                return;

            var sourceColumns = new List<string> { FieldMappingItem.IgnoreSourceColumn };
            foreach (DataColumn column in table.Columns)
            {
                if (string.Equals(column.ColumnName, ImportWizardController.HasErrorColumnName, StringComparison.Ordinal))
                    continue;

                sourceColumns.Add(column.ColumnName);
            }

            foreach (var field in _fieldDefinitions)
            {
                var expectedSource = !string.IsNullOrEmpty(field.SourceColumnName) ? field.SourceColumnName
                    : !string.IsNullOrEmpty(field.DisplayName) ? field.DisplayName
                    : field.FieldName;
                var selected = sourceColumns.FirstOrDefault(column =>
                    string.Equals(column, expectedSource, StringComparison.OrdinalIgnoreCase))
                    ?? FieldMappingItem.IgnoreSourceColumn;
                var item = new FieldMappingItem(field.FieldName, field.DisplayName, field.Description, sourceColumns, selected);
                item.PropertyChanged += OnMappingItemChanged;
                Mapping.Add(item);
            }
        }

        /// <summary>
        /// Programa una revalidación silenciosa cuando el usuario cambia el mapeo de un
        /// campo en el grid; el temporizador de debounce coalesce las ediciones sucesivas.
        /// </summary>
        private void OnMappingItemChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(FieldMappingItem.SelectedSourceColumn))
                return;

            ScheduleSilentRevalidation();
        }

        /// <summary>
        /// Compone las definiciones activas para validar: clona las definiciones de campo
        /// y les asigna la columna origen seleccionada en el grid de mapeo. Los campos
        /// marcados como <c>(Ignorar)</c> reciben un origen ficticio para que el gestor no
        /// encuentre datos y el campo quede excluido del resultado (paridad exacta con el
        /// asistente WinForms). Se invoca por tracción desde el orquestador en cada
        /// validación, por lo que siempre devuelve una colección fresca.
        /// </summary>
        private FieldDefinitionCollection BuildActiveDefinitions()
        {
            var definitions = _fieldDefinitions.Clone();
            foreach (var mapping in Mapping)
            {
                if (!definitions.TryGetDefinition(mapping.FieldName, out var definition) || definition is null)
                    continue;

                if (string.IsNullOrEmpty(mapping.SelectedSourceColumn) ||
                    mapping.SelectedSourceColumn == FieldMappingItem.IgnoreSourceColumn)
                {
                    definition.SourceColumnName = definition.DisplayName;
                }
                else
                {
                    definition.SourceColumnName = mapping.SelectedSourceColumn;
                }
            }

            return definitions;
        }

        /// <summary>
        /// Presenta un error crítico por el servicio de mensajes, con los detalles técnicos
        /// completos en la sección expandible del diálogo.
        /// </summary>
        private void NotifyCriticalError(string caption, string heading, Exception exception)
        {
            _ = _messageService?.ShowAsync(new MessageArgs
            {
                Caption = caption,
                Heading = heading,
                Message = exception.Message,
                Expander = exception.ToString(),
                MessageType = MessageType.Error,
                MessageIcon = MessageIcon.Error
            });
        }
    }
}
