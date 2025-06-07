using KUtilitiesCore.MVVM.Command;
using System;
using System.Linq;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Define la funcionalidad para el manejo y actualización de los comandos en un ViewModel.
    /// </summary>
    /// <remarks>
    /// Esta interfaz proporciona métodos para registrar comandos que se actualizan automáticamente y para notificar a los comandos registrados que su estado de ejecución puede haber cambiado.
    /// Normalmente se utiliza en clases ViewModel para asegurar que los comandos estén correctamente sincronizados con el estado de la aplicación.
    /// </remarks>
    public interface ISupportCommands
    {
        /// <summary>
        /// Registra un comando para que su estado CanExecute sea re-evaluado automáticamente cuando una propiedad del ViewModel
        /// cambie.
        /// </summary>
        /// <typeparam name="TCommand">Tipo de comando a registrar.</typeparam>
        /// <param name="command">El comando a registrar.</param>
        void RegisterCommand<TCommand>(TCommand command) where TCommand : RelayCommandBase;

        /// <summary>
        /// Itera sobre los comandos registrados y notifica que su estado de ejecución puede haber cambiado.
        /// </summary>
        void UpdateRegisteredCommands();
    }
}
