using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.Paging
{
    /// <summary>
    /// Representa las opciones de configuración para realizar una solicitud de datos paginados.
    /// </summary>
    public interface IPagingOptions
    {
        /// <summary>
        /// Obtiene el número de la página solicitada (basado en 1).
        /// Este valor debe ser mayor que 0.
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Obtiene el número de elementos que se incluirán en cada página.
        /// Este valor debe ser mayor que 0.
        /// </summary>
        int PageSize { get; }


        /// <summary>
        /// Indica si se debe omitir la paginación.
        /// Si es true, se ignorarán PageNumber y PageSize y se devolverán todos los resultados
        /// que coincidan con la especificación (después de aplicar filtros y ordenación).
        /// Por defecto es false.
        /// </summary>
        bool SkipPagination { get; }
    }
}
