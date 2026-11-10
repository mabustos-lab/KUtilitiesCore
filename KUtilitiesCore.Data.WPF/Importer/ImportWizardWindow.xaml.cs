using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Wizard;
using KUtilitiesCore.Data.WPF.MessageService;
using KUtilitiesCore.MVVM.MessageService;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace KUtilitiesCore.Data.WPF.Importer;

/// <summary>
/// Ventana modal del asistente de importación de datos para WPF: selección de archivo,
/// configuración de análisis (CSV/Excel), mapeo de columnas, vista previa editable con
/// señalización de errores y ejecución de la importación.
/// </summary>
/// <remarks>
/// <para>
/// Vista fina sobre <see cref="ImportWizardViewModel"/>: todo el estado y las acciones viven
/// en el ViewModel, que a su vez delega la orquestación en la capa compartida
/// (<see cref="ImportWizardController"/>). Paridad funcional con el
/// <c>ImportWizardForm</c> de WinForms.
/// </para>
/// <para>
/// <b>Contrato de consumo modal</b>: la ventana se muestra con <c>ShowDialog</c>; una
/// importación exitosa con <see cref="AutoCloseOnSuccess"/> activo cierra la ventana con
/// <c>DialogResult = true</c> (equivalente al <c>DialogResult.OK</c> del formulario WinForms).
/// Al cerrarse, la ventana libera el ViewModel (temporizador y gestor de importación), pero
/// <see cref="ResultData"/> permanece expuesto y legible para que el consumidor lea el
/// resultado tipado tras el cierre — contrato del spec import-wizard.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var ventana = new ImportWizardWindow(definiciones);
/// if (ventana.ShowDialog() == true)
/// {
///     DataTable? resultado = ventana.ResultData;
/// }
/// </code>
/// </example>
public partial class ImportWizardWindow : Window
{
    private const string OpenFileFilter = "Todos los archivos soportados|*.csv;*.tsv;*.psv;*.txt;*.xlsx;*.xls|Archivos de Texto (*.csv, *.tsv;*.psv;*.txt)|*.csv;*.tsv;*.psv;*.txt|Excel (*.xlsx, *.xls)|*.xlsx;*.xls";

    private readonly ImportWizardViewModel _viewModel;
    private readonly DispatcherDebounceTimer _debounceTimer;

    /// <summary>
    /// Inicializa la ventana con las definiciones de campo, un gestor de importación opcional
    /// y un servicio de mensajes opcional con el que el ViewModel notifica fallos y resultados.
    /// </summary>
    /// <param name="fieldDefinitions">Definiciones de campo que rigen el mapeo y la validación.</param>
    /// <param name="importManager">Gestor de importación; si se omite, el ViewModel crea uno interno.</param>
    /// <param name="messageService">Servicio de mensajes; si se omite, se usa
    /// <see cref="WpfMessageService"/>, el diálogo propio con anatomía TaskDialog.</param>
    /// <remarks>
    /// La ventana inyecta un <see cref="DispatcherDebounceTimer"/> al orquestador (vía
    /// ViewModel) y se suscribe a su disparo antes de crear el ViewModel: en cada ventana
    /// de estabilización vencida, la confirmación de la edición pendiente corre primero y
    /// la revalidación del orquestador lee la tabla ya consolidada — mismo orden de
    /// suscripción que el asistente WinForms con su temporizador.
    /// </remarks>
    public ImportWizardWindow(
        FieldDefinitionCollection fieldDefinitions,
        ImportManager? importManager = null,
        ISupportMessageService? messageService = null)
    {
        InitializeComponent();
        _debounceTimer = new DispatcherDebounceTimer();
        _debounceTimer.Elapsed += OnDebounceCommitPendingEdit;
        _viewModel = new ImportWizardViewModel(
            fieldDefinitions, importManager, _debounceTimer, messageService ?? new WpfMessageService());
        DataContext = _viewModel;
        _viewModel.OpenFileRequested += OnOpenFileRequested;
        _viewModel.CloseRequested += OnCloseRequested;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closed += OnWindowClosed;
        UpdatePreviewFilter();
    }

    /// <summary>ViewModel del asistente, expuesto para escenarios avanzados de hosting.</summary>
    public ImportWizardViewModel ViewModel => _viewModel;

    /// <summary>Resultado tipado de la importación; sigue legible tras cerrar la ventana (contrato del asistente).</summary>
    public DataTable? ResultData => _viewModel.ResultData;

    /// <summary>Cierra la ventana automáticamente tras una importación exitosa; activado por omisión.</summary>
    public bool AutoCloseOnSuccess
    {
        get => _viewModel.AutoCloseOnSuccess;
        set => _viewModel.AutoCloseOnSuccess = value;
    }

    /// <summary>Ruta del archivo a importar; asignar vacío reinicia el asistente.</summary>
    public string FileName
    {
        get => _viewModel.FileName;
        set => _viewModel.FileName = value;
    }

    /// <summary>Muestra el diálogo de apertura de archivo y asigna la ruta seleccionada al ViewModel.</summary>
    /// <remarks>
    /// Responde a <see cref="ImportWizardViewModel.OpenFileRequested"/>: la ventana es dueña de los
    /// diálogos nativos (<see cref="Microsoft.Win32.OpenFileDialog"/>), el ViewModel permanece
    /// agnóstico de la presentación.
    /// </remarks>
    private void OnOpenFileRequested(object? sender, EventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = OpenFileFilter,
            DefaultExt = ".xlsx",
            AddExtension = true
        };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.FileName = dialog.FileName;
        }
    }

    /// <summary>Cierra la ventana con veredicto afirmativo tras una importación exitosa.</summary>
    private void OnCloseRequested(object? sender, EventArgs e)
        => CloseWithDialogResult(true);

    private void OnCancelClick(object sender, RoutedEventArgs e)
        => CloseWithDialogResult(false);

    /// <summary>
    /// Cierra la ventana registrando el veredicto del diálogo cuando la ventana es modal.
    /// </summary>
    /// <remarks>
    /// <see cref="Window.DialogResult"/> solo es asignable en ventanas mostradas con
    /// <c>ShowDialog</c>: una ventana no modal se cierra sin veredicto (el consumidor decidió
    /// no esperar un resultado).
    /// </remarks>
    private void CloseWithDialogResult(bool? result)
    {
        try
        {
            DialogResult = result;
        }
        catch (InvalidOperationException)
        {
        }

        Close();
    }

    /// <summary>Oculta la columna de control <c>_HasError</c> de la vista previa generada automáticamente.</summary>
    /// <remarks>Paridad con el formulario WinForms, que también la oculta del grid de vista previa.</remarks>
    private void OnPreviewAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (string.Equals(e.PropertyName, ImportWizardController.HasErrorColumnName, StringComparison.Ordinal))
        {
            e.Cancel = true;
        }
    }

    /// <summary>Repinta la señalización de errores y sincroniza el filtro de la vista previa al cambiar el estado del ViewModel.</summary>
    /// <remarks>
    /// El pipeline pinta los errores sobre las mismas instancias de fila sin disparar
    /// eventos de WPF: el ViewModel notifica el veredicto (<c>RowErrors</c>/
    /// <c>ResultData</c>) y la vista repinta las filas realizadas con
    /// <see cref="DataGridErrorBehavior.Refresh"/>; <c>LoadingRow</c> cubre las filas
    /// que aún no se materializan y los cambios completos de fuente
    /// (<c>LoadedDataView</c>). El estado del filtro (<c>ShowErrorsOnly</c>) y las
    /// cargas nuevas reconstruyen la fuente de la vista previa vía
    /// <see cref="UpdatePreviewFilter"/>.
    /// </remarks>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ImportWizardViewModel.ShowErrorsOnly)
            or nameof(ImportWizardViewModel.LoadedDataView))
        {
            UpdatePreviewFilter();
        }

        if (e.PropertyName is nameof(ImportWizardViewModel.RowErrors)
            or nameof(ImportWizardViewModel.ResultData)
            or nameof(ImportWizardViewModel.LoadedDataView))
        {
            DataGridErrorBehavior.Refresh(dgvPreview);
        }
    }

    /// <summary>
    /// Aplica o retira el filtro de la vista previa según el estado de
    /// "Mostrar solo filas con errores".
    /// </summary>
    /// <remarks>
    /// Paridad con <c>FilterHasErrors</c> del asistente WinForms: con el filtro activo la
    /// vista previa expone un <see cref="DataView"/> con la condición
    /// <c>[_HasError] = true</c> producida por el orquestador compartido; sin filtro se
    /// muestra la vista completa de los datos cargados. Al cambiar la fuente, el
    /// <see cref="DataGrid"/> re-materializa las filas y el comportamiento adjunto de
    /// errores las repinta vía <c>LoadingRow</c>.
    /// </remarks>
    private void UpdatePreviewFilter()
    {
        var table = _viewModel.LoadedDataView?.Table;
        dgvPreview.ItemsSource = _viewModel.ShowErrorsOnly && table is not null
            ? ImportWizardController.CreateErrorsOnlyView(table)
            : _viewModel.LoadedDataView;
    }

    /// <summary>
    /// Programa una revalidación silenciosa al terminar la edición de una celda de la
    /// vista previa.
    /// </summary>
    /// <remarks>
    /// Paridad con <c>dgvPreview_CellEndEdit</c> del asistente WinForms: el temporizador
    /// de debounce coalesce las ediciones sucesivas; al vencer la ventana de
    /// estabilización, <see cref="OnDebounceCommitPendingEdit"/> confirma la edición
    /// pendiente y el orquestador revalida sobre la tabla ya consolidada.
    /// </remarks>
    private void OnPreviewCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        => _viewModel.ScheduleSilentRevalidation();

    /// <summary>
    /// Confirma la edición pendiente de la vista previa antes de que el comando de
    /// importación se ejecute.
    /// </summary>
    /// <remarks>
    /// <see cref="System.Windows.Controls.Primitives.ButtonBase"/> eleva el evento
    /// <c>Click</c> y después ejecuta el <c>Command</c> enlazado, por lo que este
    /// manejador corre primero y la importación siempre valida la tabla consolidada
    /// (paridad con el <c>CommitPendingGridEdit</c> al inicio de <c>ImportData</c> del
    /// asistente WinForms).
    /// </remarks>
    private void OnImportClick(object sender, RoutedEventArgs e)
        => CommitPendingGridEdit();

    /// <summary>
    /// Confirma la edición pendiente de la vista previa al vencer la ventana de
    /// estabilización, antes de que el orquestador revalide.
    /// </summary>
    /// <remarks>
    /// La ventana se suscribe a <see cref="DispatcherDebounceTimer.Elapsed"/> antes de
    /// crear el ViewModel, de modo que en cada disparo este confirmador corre primero
    /// y la revalidación del orquestador lee la tabla ya consolidada (mismo orden de
    /// suscripción que el asistente WinForms).
    /// </remarks>
    private void OnDebounceCommitPendingEdit(object? sender, EventArgs e)
        => CommitPendingGridEdit();

    /// <summary>
    /// Confirma la edición en curso de la vista previa para que el valor editado se
    /// escriba en el <see cref="DataView"/> antes de cualquier validación.
    /// </summary>
    /// <remarks>
    /// Equivalente WPF de <c>CommitPendingGridEdit</c> (que usaba
    /// <c>CurrencyManager.EndCurrentEdit</c> en WinForms): el
    /// <see cref="DataGrid"/> puede retener una celda en modo de edición; sin esta
    /// confirmación la validación leería el valor previo de la fila.
    /// </remarks>
    private void CommitPendingGridEdit()
        => dgvPreview.CommitEdit(DataGridEditingUnit.Row, true);

    /// <summary>Desconecta los manejadores de vista y libera el ViewModel al cerrar la ventana.</summary>
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _viewModel.OpenFileRequested -= OnOpenFileRequested;
        _viewModel.CloseRequested -= OnCloseRequested;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _debounceTimer.Elapsed -= OnDebounceCommitPendingEdit;
        Closed -= OnWindowClosed;
        _viewModel.OnDestroy();
    }
}
