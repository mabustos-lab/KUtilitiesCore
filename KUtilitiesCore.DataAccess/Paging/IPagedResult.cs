using System;
using System.Linq;

namespace KUtilitiesCore.DataAccess.Paging
{
    /// <summary>
    /// Representa el resultado de una consulta paginada.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de entidad en la colección.</typeparam>
    public interface IPagedResult<out TEntity> // Usamos 'out' para covarianza
    {
        /// <summary>
        /// Lista de solo lectura de elementos para la página actual.
        /// Requiere .NET Framework 4.5 o superior.
        /// </summary>
        IReadOnlyList<TEntity> Items { get; }

        /// <summary>
        /// Número de la página actual devuelta (basado en 1).
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Tamaño de página solicitado.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Número total de elementos que coinciden con la consulta (sin paginar).
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Número total de páginas disponibles.
        /// </summary>
        int TotalPages { get; }

        /// <summary>
        /// Indica si existe una página anterior.
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Indica si existe una página siguiente.
        /// </summary>
        bool HasNextPage { get; }
    }
}
