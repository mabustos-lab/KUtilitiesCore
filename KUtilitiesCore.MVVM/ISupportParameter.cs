using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Define una interfaz para proporcionar soporte de parametrización en un objeto ViewModel.
    /// </summary>
    public interface ISupportParameter
    {
        /// <summary>
        /// Obtiene o establece el parámetro asociado al ViewModel.
        /// Este parámetro puede ser utilizado para pasar datos o configuraciones específicas al ViewModel.
        /// </summary>
        object? Parameter { get; set; }
    }
}
