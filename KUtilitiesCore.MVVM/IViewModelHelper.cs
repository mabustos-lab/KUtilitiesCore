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
    public interface IViewModelHelper
    {
        /// <summary>
        /// Indica cuando el modelo está realizando un proceso 
        /// </summary>
        bool IsLoading { get; set; }

    }
}
