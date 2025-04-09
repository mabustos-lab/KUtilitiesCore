using KUtilitiesCore.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.OrderedInfo
{
    /// <summary>
    /// Establece la dirección de ordenamiento
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Ordena una secuencia de forma ascendente
        /// </summary>
        [Display(Name = "Ascendente")]
        Ascending,

        /// <summary>
        /// Ordena una secuencia de forma descendente
        /// </summary>
        [Display(Name = "Descendente")]
        Descending
    }
}
