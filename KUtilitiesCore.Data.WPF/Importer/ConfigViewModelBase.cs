using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.Wizard.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KUtilitiesCore.Data.WPF.Importer
{
    /// <summary>
    /// Base compartida para los ViewModel de configuración de análisis de archivos.
    /// Implementa el contrato de configuración compartido (<see cref="IImportConfigControl"/>)
    /// de forma agnóstica del framework de interfaz, de modo que el orquestador del asistente
    /// y la ventana WPF puedan consumir las opciones sin depender de controles concretos.
    /// </summary>
    /// <remarks>
    /// <see cref="OptionsChanged"/> se emite únicamente por interacción real del usuario,
    /// nunca durante la restauración automática de valores que realiza <see cref="Initialize"/>
    /// (los derivados usan <see cref="SuppressOptionsChanged"/> para restaurar sin notificar).
    /// Como doble protección, el host se suscribe a <see cref="OptionsChanged"/> después de
    /// invocar <see cref="Initialize"/>, replicando el contrato del asistente WinForms.
    /// </remarks>
    public abstract class ConfigViewModelBase : INotifyPropertyChanged, IImportConfigControl
    {
        private bool _suppressOptionsChanged;

        /// <summary>
        /// Se dispara cuando el usuario modifica una opción que afecta el análisis del archivo.
        /// El host lo usa para invalidar los datos cargados y exigir una recarga.
        /// </summary>
        public event EventHandler? OptionsChanged;

        /// <summary>
        /// Se dispara cuando cambia el valor de una propiedad de configuración.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public abstract void Initialize(string fileName);

        /// <inheritdoc/>
        public abstract IParsingOptions GetParsingOptions();

        /// <summary>
        /// Notifica a los suscriptores que cambió el valor de la propiedad indicada.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad que cambió (inferido por el compilador).</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Asigna el campo respaldo si el valor realmente cambió, notifica
        /// <see cref="PropertyChanged"/> y emite <see cref="OptionsChanged"/> cuando la
        /// notificación no está suprimida.
        /// </summary>
        /// <typeparam name="T">Tipo del valor de la propiedad.</typeparam>
        /// <param name="field">Campo respaldo de la propiedad.</param>
        /// <param name="value">Nuevo valor a asignar.</param>
        /// <param name="propertyName">Nombre de la propiedad (inferido por el compilador).</param>
        /// <returns><see langword="true"/> si el valor cambió y se asignó; <see langword="false"/> si ya era igual.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            RaisePropertyChanged(propertyName);
            if (!_suppressOptionsChanged)
                OptionsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Ejecuta la acción restaurando valores de configuración sin emitir
        /// <see cref="OptionsChanged"/>, para no marcar los datos como obsoletos durante
        /// la inicialización automática del control.
        /// </summary>
        /// <param name="action">Acción de restauración a ejecutar.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="action"/> es <see langword="null"/>.</exception>
        protected void SuppressOptionsChanged(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);
            _suppressOptionsChanged = true;
            try
            {
                action();
            }
            finally
            {
                _suppressOptionsChanged = false;
            }
        }
    }
}
