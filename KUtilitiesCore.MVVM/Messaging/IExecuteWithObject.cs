using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Esta interfaz está destinada para el <see cref="WeakAction{T}"/> clase y puede ser útil si
    /// almacena múltiples <see cref="WeakReference{T}"/> {t} pero no se sabe de antemano qué El
    /// tipo T representa.
    /// </summary>
    public interface IExecuteWithObject
    {
        /// <summary>
        /// Ejecuta una acción.
        /// </summary>
        /// <param name="parameter">
        /// Un parámetro pasó como un objeto, para ser modeado al tipo apropiado.
        /// </param>
        void ExecuteWithObject(object parameter);
    }
}
