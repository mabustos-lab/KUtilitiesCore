using KUtilitiesCore.Data.Wizard;

namespace KUtilitiesCore.Data.WPFTests.TestDoubles;

/// <summary>
/// Temporizador de debounce controlado manualmente para pruebas: <see cref="Fire"/> dispara
/// <see cref="IWizardDebounceTimer.Elapsed"/> una sola vez por ventana (un disparo por cada
/// arranque), imitando la semántica de "exactamente una revalidación por ventana de
/// estabilización" que el orquestador garantiza al detener el temporizador dentro de su
/// propio manejo de <c>Elapsed</c>.
/// </summary>
internal sealed class FakeDebounceTimer : IWizardDebounceTimer
{
    private int _lastIntervalMilliseconds;

    /// <summary>Se dispara al invocar <see cref="Fire"/> con el temporizador activo.</summary>
    public event EventHandler? Elapsed;

    /// <summary>Indica si hay una ventana de estabilización activa (arrancada y no disparada).</summary>
    public bool IsRunning { get; private set; }

    /// <summary>Número de arranques registrados (uno por cada <see cref="IWizardDebounceTimer.Start"/>).</summary>
    public int StartCallCount { get; private set; }

    /// <summary>Número de detenciones efectivas (con el temporizador activo).</summary>
    public int StopCallCount { get; private set; }

    /// <summary>Número de disparos reales de <see cref="Elapsed"/> (uno por ventana).</summary>
    public int FireCount { get; private set; }

    /// <summary>Intervalo (ms) del último <see cref="IWizardDebounceTimer.Start"/> recibido.</summary>
    public int LastIntervalMilliseconds => _lastIntervalMilliseconds;

    /// <summary>Arranca la ventana de estabilización reiniciándola si ya estaba activa (coalescencia).</summary>
    /// <param name="intervalMilliseconds">Intervalo registrado (el fake no mide tiempo, solo lo expone).</param>
    public void Start(int intervalMilliseconds)
    {
        IsRunning = true;
        StartCallCount++;
        _lastIntervalMilliseconds = intervalMilliseconds;
    }

    /// <summary>Cancela la ventana pendiente sin disparar; no hace nada si estaba inactivo.</summary>
    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        StopCallCount++;
    }

    /// <summary>Dispara <see cref="Elapsed"/> una sola vez si la ventana está activa; no-op si fue cancelada.</summary>
    /// <remarks>Tras el disparo la ventana queda inactiva: cada arranque produce exactamente un disparo.</remarks>
    public void Fire()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        FireCount++;
        Elapsed?.Invoke(this, EventArgs.Empty);
    }
}
