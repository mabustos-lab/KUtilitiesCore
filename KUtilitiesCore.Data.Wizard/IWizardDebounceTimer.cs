namespace KUtilitiesCore.Data.Wizard;

/// <summary>
/// Abstracción del temporizador de debounce usado por el orquestador del asistente de importación.
/// </summary>
/// <remarks>
/// <para>
/// Permite reprogramar la revalidación silenciosa tras cada edición del usuario sin acoplar la
/// capa de orquestación a un temporizador de UI concreto: WinForms envuelve
/// <c>System.Windows.Forms.Timer</c>, WPF envuelve <c>DispatcherTimer</c> y las pruebas usan un
/// fake de disparo manual determinista.
/// </para>
/// <para>
/// Contrato de hilo: las implementaciones de UI deben disparar <see cref="Elapsed"/> en el hilo
/// de UI del consumidor, porque la revalidación edita <c>DataTable</c> (afinidad de un solo
/// hilo).
/// </para>
/// </remarks>
public interface IWizardDebounceTimer
{
    /// <summary>
    /// Se dispara cuando vence la ventana de estabilización programada con <see cref="Start"/>.
    /// </summary>
    event EventHandler? Elapsed;

    /// <summary>
    /// Programa el disparo tras el intervalo indicado; reinicia la ventana si ya había una
    /// programación activa (coalescencia de ediciones sucesivas).
    /// </summary>
    /// <param name="intervalMilliseconds">Ventana de estabilización en milisegundos.</param>
    void Start(int intervalMilliseconds);

    /// <summary>
    /// Cancela la programación activa sin disparar <see cref="Elapsed"/>.
    /// </summary>
    /// <remarks>
    /// Detener un temporizador inactivo es un no-op: el orquestador llama a <c>Stop()</c> antes
    /// de cada <c>Start</c> para garantizar coalescencia sin estados previos.
    /// </remarks>
    void Stop();
}
