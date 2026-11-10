using KUtilitiesCore.Data.Wizard;

namespace KUtilitiesCore.Data.Win.Importer
{
    /// <summary>
    /// Adaptador WinForms de <see cref="IWizardDebounceTimer"/>: envuelve un
    /// <see cref="System.Windows.Forms.Timer"/> para que el orquestador compartido
    /// del asistente pueda programar revalidaciones silenciosas sin depender de un
    /// framework de interfaz. El disparo ocurre en el hilo de UI del consumidor,
    /// requisito del orquestador por la afinidad de hilos de
    /// <see cref="System.Data.DataTable"/>.
    /// </summary>
    /// <remarks>
    /// El temporizador subyacente se crea sin iniciarse: el orquestador decide cuándo
    /// abrir la ventana de estabilización mediante <see cref="Start"/>. Implementa
    /// <see cref="IDisposable"/> para que el orquestador lo libere junto con su
    /// propio ciclo de vida.
    /// </remarks>
    public sealed class WinFormsDebounceTimer : IWizardDebounceTimer, IDisposable
    {
        private readonly System.Windows.Forms.Timer _timer;

        /// <summary>
        /// Se dispara cuando vence la ventana de estabilización programada con
        /// <see cref="Start"/>. Reenvía el evento <see cref="System.Windows.Forms.Timer.Tick"/>
        /// del temporizador subyacente.
        /// </summary>
        public event EventHandler? Elapsed
        {
            add => _timer.Tick += value;
            remove => _timer.Tick -= value;
        }

        /// <summary>
        /// Crea el temporizador subyacente sin iniciarlo.
        /// </summary>
        public WinFormsDebounceTimer()
        {
            _timer = new System.Windows.Forms.Timer();
        }

        /// <inheritdoc/>
        public void Start(int intervalMilliseconds)
        {
            _timer.Stop();
            _timer.Interval = intervalMilliseconds;
            _timer.Start();
        }

        /// <inheritdoc/>
        public void Stop() => _timer.Stop();

        /// <summary>
        /// Libera el temporizador subyacente; el orquestador lo invoca desde su
        /// <see cref="IDisposable.Dispose"/> al reconocer este adaptador como desechable.
        /// </summary>
        public void Dispose() => _timer.Dispose();
    }
}
