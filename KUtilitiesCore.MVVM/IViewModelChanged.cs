using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Funcionalizad usada para establecer los valores de una propiedad despues de haber cambiado
    /// </summary>
    public interface IViewModelChanged
    {
        /// <summary>
        /// Genera un evento que indica que una propiedad cambio
        /// </summary>
        /// <param name="propertyName"></param>
        void RaisePropertyChanged([CallerMemberName] string propertyName = "");
    }
}
