using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Funcionalizad usada para establecer los valores de una propiedad antes de haber cambiado
    /// </summary>
    public interface IViewModelChanging
    {
        /// <summary>
        /// Genera un evento que indica que una propiedad va a cambiar
        /// </summary>
        /// <param name="propertyName"></param>
        void RaisePropertyChanging([CallerMemberName] string propertyName = "");
    }
}
