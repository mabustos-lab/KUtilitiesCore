using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using System.Data;

namespace KUtilitiesCore.Data.Wizard;

/// <summary>
/// Orquestador del ciclo de vida del asistente de importación de datos, agnóstico del framework
/// de interfaz de usuario. Concentra el estado lógico (archivo seleccionado, datos cargados,
/// carga en curso, datos obsoletos, resultado tipado) y las operaciones (cargar, revalidar,
/// invalidar, reiniciar) para que ambos front-ends (WinForms y WPF) observen el mismo estado
/// a través de la misma capa.
/// </summary>
/// <remarks>
/// <para>
/// <b>Contrato de afinidad de hilo</b>: todo el estado del orquestador debe manipularse y
/// observarse desde el hilo del consumidor (típicamente el hilo de UI).
/// <see cref="LoadAsync(IDataSourceReader)"/> deliberadamente NO usa
/// <c>ConfigureAwait(false)</c> para que la continuación regrese al contexto del llamador:
/// <see cref="DataTable"/> y <see cref="DataView"/> tienen afinidad de un solo hilo y no
/// requieren bloqueos bajo este contrato. No se usan locks por diseño.
/// </para>
/// <para>
/// <b>Propiedad de la tabla cargada</b>: la tabla devuelta por el lector se adopta sin clonar
/// y sigue siendo propiedad del llamador; <see cref="Dispose"/> NO la dispone. En cambio,
/// <see cref="ResultData"/> es creado por el orquestador (proyección tipada): la instancia
/// anterior se dispone al reemplazarse y, al liberar el orquestador, la tabla del resultado
/// se dispone conservando la referencia expuesta para que el consumidor pueda leerla tras
/// cerrar y disponer la vista (contrato del spec <c>import-wizard</c>).
/// </para>
/// <para>
/// El constructor toma posesión del <see cref="ImportManager"/> (proporcionado o creado
/// internamente) y lo dispone en <see cref="Dispose"/>, replicando la semántica del
/// formulario WinForms original que disponía incluso los managers inyectados.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var controller = new ImportWizardController(definitions, debounceTimer: new FakeDebounceTimer());
/// controller.LoadFailed += (_, e) => Console.WriteLine(e.Exception.Message);
/// bool loaded = await controller.LoadAsync(reader);
/// </code>
/// </example>
public sealed class ImportWizardController : IDisposable
{
    /// <summary>Intervalo por omisión (ms) de la ventana de estabilización del debounce.</summary>
    /// <remarks>Paridad con el temporizador del formulario WinForms original (400 ms); configurable al programar la revalidación silenciosa.</remarks>
    public const int DefaultDebounceIntervalMilliseconds = 400;

    /// <summary>Nombre de la columna de control que marca las filas con error para el filtrado de vistas.</summary>
    /// <remarks>Columna paridad con la del formulario WinForms original: los front-ends la ocultan en sus grids y la usan como filtro.</remarks>
    public const string HasErrorColumnName = "_HasError";

    private readonly ImportManager _importManager;
    private readonly IWizardDebounceTimer? _debounceTimer;
    private readonly FieldDefinitionCollection _fieldDefinitions;

    private string _fileName = string.Empty;
    private DataView? _loadedDataView;
    private DataTable? _resultData;
    private IReadOnlyList<WizardRowError> _rowErrors = Array.Empty<WizardRowError>();

    /// <summary>Ruta del archivo en importación; vacío significa sin selección.</summary>
    /// <remarks>Asignar <see langword="null"/> normaliza a vacío; asignar vacío reinicia el estado lógico del asistente. El hosting de paneles de configuración es responsabilidad de la vista.</remarks>
    public string FileName
    {
        get => _fileName;
        set
        {
            var normalized = value ?? string.Empty;
            if (string.Equals(_fileName, normalized, StringComparison.Ordinal))
            {
                return;
            }

            _fileName = normalized;
            if (_fileName.Length == 0)
            {
                Reset();
            }
        }
    }

    /// <summary>Vista sobre la tabla adoptada del lector; <see langword="null"/> si no hay carga.</summary>
    /// <remarks>La tabla subyacente es propiedad del llamador: el orquestador no la clona ni la dispone.</remarks>
    public DataView? LoadedDataView => _loadedDataView;

    /// <summary>Indica si hay una carga en curso; una segunda carga se rechaza mientras esté activa.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>Indica que la configuración cambió tras la última carga y los datos deben recargarse.</summary>
    /// <remarks>Se restablece al completar una carga o al reiniciar; se activa por invalidación (tarea 4).</remarks>
    public bool IsDataStale { get; private set; }

    /// <summary>Proyección tipada del resultado de importación; <see langword="null"/> si la última validación falló o no hay carga.</summary>
    /// <remarks>
    /// <para>
    /// Tabla creada por el orquestador: la instancia anterior se dispone al reemplazarse
    /// (revalidación, invalidación o reset).
    /// </para>
    /// <para>
    /// Tras <see cref="Dispose"/> la referencia sigue expuesta con la tabla dispuesta pero
    /// legible (paridad con el <c>OnDispose</c> del formulario WinForms original, que disponía
    /// sin nullar): los datos de una <see cref="DataTable"/> dispuesta permanecen accesibles,
    /// de modo que el consumidor puede leer el resultado después de cerrar y disponer la vista
    /// — escenario "resultado utilizable tras cerrar el asistente" del spec import-wizard.
    /// </para>
    /// </remarks>
    public DataTable? ResultData => _resultData;

    /// <summary>Indica si la operación de importación puede ejecutarse (datos cargados con filas y no obsoletos).</summary>
    public bool CanImport { get; private set; }

    /// <summary>Proyección neutral de los errores de la última revalidación: (fila, campo visible, mensaje).</summary>
    /// <remarks>
    /// Vacía si no se ha revalidado o la última revalidación fue exitosa. Los front-ends la
    /// consumen para pintar sus vistas de errores sin acoplarse al gestor de importación.
    /// </remarks>
    public IReadOnlyList<WizardRowError> RowErrors => _rowErrors;

    /// <summary>
    /// Origen del mapeo activo que la revalidación debe aplicar: cada invocación debe devolver
    /// una colección fresca (idealmente sobre clones de las definiciones originales).
    /// </summary>
    /// <remarks>
    /// <para>
    /// El mapeo activo — qué columna origen alimenta cada campo destino — vive en la UI de cada
    /// front-end (grid de mapeo WinForms/WPF), fuera del alcance de esta capa agnóstica. El
    /// resolver permite consumirlo en el momento exacto de validar: la semántica es de extracción
    /// (pull), no de instantánea, porque el grid cambia sin notificar al orquestador.
    /// </para>
    /// <para>
    /// Si se deja en <see langword="null"/> (o el front-end no tiene mapeo dirigido por usuario),
    /// la revalidación usa un clon de las definiciones originales, que es el mapeo directo
    /// FieldName/SourceColumnName sin correcciones del usuario.
    /// </para>
    /// </remarks>
    public Func<FieldDefinitionCollection>? ActiveDefinitionsResolver { get; set; }

    /// <summary>Se dispara cuando una carga termina con éxito, incluso con tablas de cero filas.</summary>
    /// <remarks>El front-end decide cómo advertir sobre conteos en cero; la capa no emite mensajes.</remarks>
    public event EventHandler? LoadCompleted;

    /// <summary>Se dispara cuando una carga falla, con la excepción original para que la vista decida la presentación.</summary>
    public event EventHandler<LoadFailedEventArgs>? LoadFailed;

    /// <summary>Se dispara al terminar una importación, con el veredicto de validación y el conteo de errores.</summary>
    /// <remarks>Evento neutral: la capa no muestra diálogos; los front-ends deciden cómo informar al usuario.</remarks>
    public event EventHandler<ImportCompletedEventArgs>? ImportCompleted;

    /// <summary>Se dispara cuando la exposición neutral de errores cambia (revalidación, carga o reset).</summary>
    /// <remarks>
    /// <para>
    /// Permite a las vistas (p. ej. el ViewModel de WPF) refrescar sus grids de errores sin sondear.
    /// </para>
    /// <para>
    /// Al dispararse tras una revalidación, <see cref="RowErrors"/> y <see cref="ResultData"/> ya
    /// reflejan el veredicto completado: los manejadores pueden decidir su pintado con el estado
    /// completo, sin condiciones de carrera con la materialización del resultado.
    /// </para>
    /// </remarks>
    public event EventHandler? ErrorsChanged;

    /// <summary>
    /// Inicializa el orquestador con las definiciones de campo, un gestor de importación opcional
    /// y un temporizador de debounce opcional.
    /// </summary>
    /// <param name="fieldDefinitions">Definiciones de campo que rigen el mapeo y la validación.</param>
    /// <param name="importManager">Gestor de importación; si se omite se crea uno interno. El orquestador toma su propiedad.</param>
    /// <param name="debounceTimer">Temporizador de estabilización para revalidaciones silenciosas; opcional.</param>
    public ImportWizardController(
        FieldDefinitionCollection fieldDefinitions,
        ImportManager? importManager = null,
        IWizardDebounceTimer? debounceTimer = null)
    {
        ArgumentNullException.ThrowIfNull(fieldDefinitions);
        _fieldDefinitions = fieldDefinitions;
        _importManager = importManager ?? new ImportManager();
        _debounceTimer = debounceTimer;
        if (_debounceTimer is not null)
        {
            _debounceTimer.Elapsed += OnDebounceElapsed;
        }
    }

    /// <summary>Carga sincrónicamente los datos desde el lector y adopta la tabla resultante.</summary>
    /// <param name="reader">Lector de origen de datos.</param>
    /// <returns><see langword="true"/> si la carga terminó; <see langword="false"/> si fue rechazada por reentrada o falló.</returns>
    public bool Load(IDataSourceReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (IsLoading)
        {
            return false;
        }

        IsLoading = true;
        try
        {
            AdoptLoadedTable(reader.ReadData());
            return true;
        }
        catch (Exception ex)
        {
            OnLoadFailed(ex);
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Carga los datos de forma asincrónica manteniendo el contexto del llamador.</summary>
    /// <param name="reader">Lector de origen de datos.</param>
    /// <returns><see langword="true"/> si la carga terminó; <see langword="false"/> si fue rechazada por reentrada o falló.</returns>
    /// <remarks>La continuación se ejecuta en el contexto del llamador (sin ConfigureAwait(false)) para respetar la afinidad de hilo de DataTable/DataView.</remarks>
    public async Task<bool> LoadAsync(IDataSourceReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (IsLoading)
        {
            return await Task.FromResult(false);
        }

        IsLoading = true;
        try
        {
            var table = await reader.ReadDataAsync();
            AdoptLoadedTable(table);
            return true;
        }
        catch (Exception ex)
        {
            OnLoadFailed(ex);
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Reinicia el estado lógico del asistente sin disponer la tabla del llamador.</summary>
    /// <remarks>Detiene el debounce pendiente, libera el resultado tipado, limpia la exposición de errores y deshabilita la importación. Si hay una carga en curso, ésta repoblará el estado al completarse.</remarks>
    public void Reset()
    {
        _debounceTimer?.Stop();
        _loadedDataView = null;
        ClearResultData();
        ClearRowErrors();
        CanImport = false;
        IsDataStale = false;
    }

    /// <summary>Ejecuta el pipeline silencioso de mapeo y validación sobre la tabla adoptada.</summary>
    /// <returns><see langword="true"/> si todos los datos son válidos; <see langword="false"/> si no hay datos cargados o la validación falló.</returns>
    /// <remarks>
    /// <para>
    /// Es la contraparte sin UI de <c>RevalidateAfterEdit</c> del formulario WinForms: aplica el
    /// mapeo activo (del <see cref="ActiveDefinitionsResolver"/> si el front-end lo provee, o un
    /// clon de las definiciones originales para no mutarlas), relee la tabla adoptada <b>sin
    /// clonarla</b> (los errores quedan pintados directamente sobre la tabla del llamador,
    /// contrato de <see cref="ImportManager.ReadData(DataTable)"/>) y actualiza
    /// <see cref="ResultData"/> según el resultado.
    /// </para>
    /// <para>
    /// Es idempotente y reutilizable: la validación limpia los errores previos de las filas antes
    /// de volver a evaluarlas, por lo que las revalidaciones silenciosas del debounce pueden
    /// invocarla repetidamente. Detiene el debounce pendiente porque una validación explícita
    /// reemplaza cualquier revalidación programada.
    /// </para>
    /// </remarks>
    public bool Revalidate()
    {
        _debounceTimer?.Stop();
        var table = _loadedDataView?.Table;
        if (table is null)
        {
            return false;
        }

        _importManager.SetMapping(ResolveActiveDefinitions());
        _importManager.ReadData(table);
        var isValid = _importManager.ValidateDataTypes();
        UpdateRowErrorFlags(table);

        // El resultado tipado y la exposición de errores se materializan ANTES de notificar:
        // los manejadores de ErrorsChanged deben observar el estado completo de la
        // revalidación (Resultado y RowErrors coherentes entre sí), no un estado a medias.
        ClearResultData();
        if (isValid)
        {
            _resultData = _importManager.CreateResultTable();
        }

        RebuildRowErrors();
        ErrorsChanged?.Invoke(this, EventArgs.Empty);
        return isValid;
    }

    /// <summary>Crea una vista filtrada sobre la tabla con únicamente las filas marcadas con error.</summary>
    /// <param name="table">Tabla adoptada por el orquestador (la columna de control se asegura aquí).</param>
    /// <returns>Vista con <c>RowFilter = "[_HasError] = true"</c>, lista para enlazar a la vista previa filtrada.</returns>
    /// <remarks>Centraliza la semántica del filtro de errores que ambos front-ends comparten (paridad con <c>FilterHasErrors</c> del formulario WinForms).</remarks>
    public static DataView CreateErrorsOnlyView(DataTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        EnsureHasErrorColumn(table);
        return new DataView(table, $"[{HasErrorColumnName}] = true", string.Empty, DataViewRowState.CurrentRows);
    }

    /// <summary>Programa una revalidación silenciosa tras la ventana de estabilización del debounce.</summary>
    /// <remarks>
    /// <para>
    /// Reinicia la ventana con cada invocación (<c>Stop</c> + <c>Start</c>), de modo que ediciones
    /// sucesivas coalescen en una sola revalidación al estabilizarse el usuario. Al dispararse el
    /// temporizador, el orquestador ejecuta <see cref="Revalidate"/> exactamente una vez.
    /// </para>
    /// <para>
    /// No hace nada si no hay datos cargados (paridad con los guards del formulario original, cuyos
    /// disparadores de edición solo existen con datos presentes) o si no se inyectó temporizador.
    /// </para>
    /// </remarks>
    public void ScheduleSilentRevalidation()
    {
        if (_loadedDataView is null || _debounceTimer is null)
        {
            return;
        }

        _debounceTimer.Stop();
        _debounceTimer.Start(DefaultDebounceIntervalMilliseconds);
    }

    /// <summary>Marcos los datos cargados como obsoletos porque cambió la configuración de análisis.</summary>
    /// <remarks>
    /// <para>
    /// Contraparte de <c>OnConfigOptionsChanged</c> del formulario WinForms: la vista invoca este
    /// método desde su manejo de <see cref="Configuration.IImportConfigControl.OptionsChanged"/>.
    /// Deshabilita la importación, retira el resultado tipado y cancela cualquier revalidación
    /// pendiente <b>sin reprogramar</b>: con configuración distinta, los datos deben recargarse,
    /// no revalidarse.
    /// </para>
    /// <para>
    /// La vigencia se restablece solo al completar una nueva carga (<see cref="Load(IDataSourceReader)"/>
    /// o <see cref="LoadAsync(IDataSourceReader)"/>).
    /// </para>
    /// </remarks>
    public void InvalidateData()
    {
        _debounceTimer?.Stop();
        IsDataStale = true;
        CanImport = false;
        ClearResultData();
    }

    /// <summary>Descarta el resultado tipado de la última revalidación sin alterar el resto del estado.</summary>
    /// <remarks>
    /// A diferencia de <see cref="InvalidateData"/> no marca los datos como obsoletos ni deshabilita
    /// la importación: los datos cargados siguen vigentes. Lo usa un front-end cuando un paso
    /// <b>posterior</b> al pipeline de validación aborta el proceso (p. ej. un hook de vista que
    /// lanza una excepción): el contrato es que un proceso abortado no deja resultado utilizable,
    /// igual que si la validación hubiera fallado.
    /// </remarks>
    public void DiscardResult()
    {
        ClearResultData();
    }

    /// <summary>Ejecuta la importación completa: revalida y notifica el resultado con el evento neutral.</summary>
    /// <returns>Argumentos con el veredicto de validación y el conteo de errores para el front-end.</returns>
    /// <remarks>
    /// Es la operación terminal del asistente: sin datos cargados devuelve un veredicto inválido
    /// (las vistas normalmente lo previenen consultando <see cref="CanImport"/> primero). Tras un
    /// veredicto válido, <see cref="ResultData"/> contiene la proyección tipada lista para consumo.
    /// </remarks>
    public ImportCompletedEventArgs Import()
    {
        var isValid = Revalidate();
        var args = new ImportCompletedEventArgs(isValid, _importManager.ValidationErrors.Errors.Count);
        ImportCompleted?.Invoke(this, args);
        return args;
    }

    /// <summary>Libera el temporizador de debounce, el resultado tipado y el gestor de importación.</summary>
    /// <remarks>
    /// <para>
    /// NO dispone la tabla cargada: es propiedad del llamador (contrato de adopción sin clonación).
    /// </para>
    /// <para>
    /// Dispone <see cref="ResultData"/> pero conserva la referencia expuesta (la tabla dispuesta
    /// sigue siendo legible), de modo que el consumidor puede leer el resultado tras cerrar y
    /// disponer la vista — paridad con el <c>OnDispose</c> del formulario WinForms original y
    /// requisito del spec import-wizard.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        if (_debounceTimer is not null)
        {
            _debounceTimer.Elapsed -= OnDebounceElapsed;
            _debounceTimer.Stop();
            if (_debounceTimer is IDisposable disposableTimer)
            {
                disposableTimer.Dispose();
            }
        }

        _resultData?.Dispose();
        _importManager.Dispose();
        _loadedDataView = null;
    }

    private void OnDebounceElapsed(object? sender, EventArgs e)
    {
        Revalidate();
    }

    /// <summary>
    /// Resuelve el mapeo activo para la revalidación: el resolver del front-end si existe, o un
    /// clon de las definiciones originales en su defecto.
    /// </summary>
    private FieldDefinitionCollection ResolveActiveDefinitions()
        => ActiveDefinitionsResolver?.Invoke() ?? _fieldDefinitions.Clone();

    private void AdoptLoadedTable(DataTable table)
    {
        UpdateRowErrorFlags(table);
        _loadedDataView = new DataView(table);
        IsDataStale = false;
        ClearResultData();
        ClearRowErrors();
        CanImport = table.Rows.Count > 0;
        LoadCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void RebuildRowErrors()
    {
        var errors = new List<WizardRowError>(_importManager.ValidationErrors.Errors.Count);
        foreach (var error in _importManager.ValidationErrors.Errors)
        {
            if (error is ValidationFailure failure)
            {
                errors.Add(new WizardRowError(
                    failure.IndexRow,
                    failure.PropertyName ?? string.Empty,
                    ResolveFieldDisplayName(failure.PropertyName),
                    failure.ErrorMessage ?? string.Empty));
            }
            else
            {
                errors.Add(new WizardRowError(-1, string.Empty, "General", error.ErrorMessage ?? string.Empty));
            }
        }

        _rowErrors = errors;
    }

    private string ResolveFieldDisplayName(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return "General";
        }

        return _fieldDefinitions.TryGetDefinition(propertyName, out var definition)
            ? definition?.DisplayName ?? propertyName
            : propertyName;
    }

    private void ClearRowErrors()
    {
        if (_rowErrors.Count == 0)
        {
            return;
        }

        _rowErrors = Array.Empty<WizardRowError>();
        ErrorsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Crea la columna de control <c>_HasError</c> si la tabla aún no la tiene.</summary>
    private static void EnsureHasErrorColumn(DataTable table)
    {
        if (!table.Columns.Contains(HasErrorColumnName))
        {
            table.Columns.Add(HasErrorColumnName, typeof(bool));
        }
    }

    /// <summary>Sincroniza la columna <c>_HasError</c> con el estado de error de cada fila.</summary>
    private static void UpdateRowErrorFlags(DataTable table)
    {
        EnsureHasErrorColumn(table);
        foreach (DataRow row in table.Rows)
        {
            row[HasErrorColumnName] = row.HasErrors;
        }
    }

    private void ClearResultData()
    {
        _resultData?.Dispose();
        _resultData = null;
    }

    private void OnLoadFailed(Exception exception)
    {
        LoadFailed?.Invoke(this, new LoadFailedEventArgs(exception));
    }
}
