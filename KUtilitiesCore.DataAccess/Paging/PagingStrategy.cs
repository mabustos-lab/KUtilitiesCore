using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.Paging
{
    /// <summary>
    /// Define la estrategia de paginación a utilizar.
    /// </summary>
    public enum PagingStrategy
    {
        /// <summary>
        /// Paginación basada en offset (Skip/Take). Puede ser menos eficiente en conjuntos de datos muy grandes.
        /// </summary>
        Offset,
        /// <summary>
        /// Paginación basada en Keyset (Seek Method / Cursor). Más eficiente para conjuntos grandes,
        /// pero requiere una columna de ordenación única o una combinación de ellas.
        /// TotalCount y TotalPages pueden no estar disponibles o ser costosos de calcular.
        /// </summary>
        Keyset
    }
}
