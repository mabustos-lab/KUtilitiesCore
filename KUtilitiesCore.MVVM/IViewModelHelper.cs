using KUtilitiesCore.MVVM.MessageService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Funcionalidades de un objeto ViewModel requeridas
    /// </summary>
    public interface IViewModelHelper: IViewModelChanged ,IViewModelChanging
    {
        /// <summary>
        /// Indica cuando el modelo está realizando un proceso 
        /// </summary>
        bool IsLoading { get; set; }
        /// <summary>
        /// Establece la funcionalidad para mostrar mensajes en la interfaz de usuario
        /// de forma asíncrona e independiente de la tecnología de UI.
        /// </summary>
        ISupportMessageService? MessageService { get; }
    }
}
