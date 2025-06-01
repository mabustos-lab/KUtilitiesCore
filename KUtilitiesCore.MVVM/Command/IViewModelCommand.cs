using System;
using System.Linq;
using System.Windows.Input;

namespace KUtilitiesCore.MVVM.Command
{
    /// <summary>
    /// Interfaz que extiende ICommand para exponer metadatos adicionales de comandos en ViewModels.
    /// </summary>
    public interface IViewModelCommand : ICommand
    {
        /// <summary>
        /// Nombre del método asociado al comando.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Indica si el comando requiere un parámetro.
        /// </summary>
        public bool IsParametrizedCommand { get; }
    }
}