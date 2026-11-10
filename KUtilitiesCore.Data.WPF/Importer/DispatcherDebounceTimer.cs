using KUtilitiesCore.Data.Wizard;
using System.Windows.Threading;

namespace KUtilitiesCore.Data.WPF.Importer;

/// <summary>
/// Adaptador WPF de <see cref="IWizardDebounceTimer"/>: envuelve un
/// <see cref="DispatcherTimer"/> para que el orquestador compartido del asistente
/// pueda programar revalidaciones silenciosas. El disparo ocurre en el hilo de UI
/// del consumidor (el <see cref="DispatcherTimer"/> encola la marca en el
/// <c>Dispatcher</c> del hilo que lo crea), requisito del orquestador por la
/// afinidad de hilos de <see cref="System.Data.DataTable"/>.
/// </summary>
/// <remarks>
/// El temporizador subyacente se crea sin iniciarse: el orquestador decide cuándo
/// abrir la ventana de estabilización mediante <see cref="Start"/>. A diferencia
/// de <c>WinFormsDebounceTimer</c>, este adaptador no implementa
/// <see cref="IDisposable"/> porque <see cref="DispatcherTimer"/> no es un recurso
/// desechable: basta con detenerlo, tarea que el orquestador ya realiza al
/// liberarse (comprueba <c>is IDisposable</c> antes de disponer el adaptador).
/// </remarks>
public sealed class DispatcherDebounceTimer : IWizardDebounceTimer
{
    private readonly DispatcherTimer _timer;

    /// <summary>
    /// Se dispara cuando vence la ventana de estabilización programada con
    /// <see cref="Start"/>. Reenvía el evento <see cref="DispatcherTimer.Tick"/>
    /// del temporizador subyacente.
    /// </summary>
    public event EventHandler? Elapsed
    {
        add => _timer.Tick += value;
        remove => _timer.Tick -= value;
    }

    /// <summary>
    /// Crea el temporizador subyacente sin iniciarlo, enlazado al
    /// <c>Dispatcher</c> del hilo creador.
    /// </summary>
    public DispatcherDebounceTimer()
    {
        _timer = new DispatcherTimer();
    }

    /// <inheritdoc/>
    public void Start(int intervalMilliseconds)
    {
        _timer.Stop();
        _timer.Interval = TimeSpan.FromMilliseconds(intervalMilliseconds);
        _timer.Start();
    }

    /// <inheritdoc/>
    public void Stop() => _timer.Stop();
}
