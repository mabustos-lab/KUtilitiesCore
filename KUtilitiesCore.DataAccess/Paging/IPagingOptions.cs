using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.Interfaces
{
    /// <summary>
    /// Define las opciones para solicitar una página de datos.
    /// </summary>
    public interface IPagingOptions
    {
        /// <summary>
        /// Número de página solicitada (basado en 1).
        /// Debe ser mayor que 0.
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Número de elementos por página.
        /// Debe ser mayor que 0.
        /// </summary>
        int PageSize { get; }

        // Opcional: Considerar añadir parámetros para ordenación aquí
        // string SortBy { get; }
        // SortDirection SortDirection { get; } // Necesitaría definir un enum SortDirection
    }
}
