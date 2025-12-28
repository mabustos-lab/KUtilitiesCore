using System.Drawing.Printing;

namespace KUtilitiesCore.Dal.Paging
{
    /// <summary>
    /// Implementación concreta del resultado paginado.
    /// </summary>
    public class PagedResult : IPagedResult
    {
        /// <summary>
        /// Constructor para inicializar el resultado paginado.
        /// </summary>
        /// <param name="totalCount">
        /// El número total de elementos. Para Keyset, puede ser -1 si no se calcula.
        /// </param>
        /// <param name="pageNumber">El número de la página actual.</param>
        /// <param name="pageSize">El tamaño de la página.</param>
        /// <param name="lastKeyValue">El valor clave del último elemento para paginación Keyset.</param>
        /// <param name="hasNextPageOverride">
        /// Permite sobreescribir el cálculo de HasNextPage, útil para Keyset.
        /// </param>
        public PagedResult(int totalCount, int pageNumber, int pageSize, object lastKeyValue = null, bool? hasNextPageOverride = null)
        {
            PageNumber = pageNumber > 0 ? pageNumber : 1;
            TotalCount = totalCount; // Para Keyset, esto podría ser el recuento de la página actual si no se conoce el total.

            // Si pageSize es 0 (y totalCount es 0), o si pageSize es igual a totalCount (caso de
            // SkipPagination), TotalPages es 1 (o 0 si no hay items).
            PageSize = pageSize > 0 ? pageSize : (totalCount == 0 ? 0 : totalCount);

            LastKeyValue = lastKeyValue;

            if (this.PageSize > 0 && TotalCount >= 0) // Solo calcular si TotalCount es válido
            {
                TotalPages = (int)Math.Ceiling(TotalCount / (double)this.PageSize);
            }
            else if (TotalCount == -1) // Keyset sin conteo total
            {
                TotalPages = -1; // Indicar desconocido
            }
            else // totalCount es 0 o PageSize es 0
            {
                TotalPages = TotalCount > 0 ? 1 : 0;
            }

            // HasNextPage: Si se proporciona un override (útil para Keyset donde se sabe si hay más
            // por el número de items devueltos vs pageSize)
            if (hasNextPageOverride.HasValue)
            {
                HasNextPage = hasNextPageOverride.Value;
            }
            // Para Offset pagination, o si TotalCount es conocido
            else if (TotalCount >= 0)
            {
                HasNextPage = PageNumber < TotalPages;
            }
            //// Para Keyset sin TotalCount, si Items.Count == PageSize, es probable que haya más.
            //// Esto es una heurística y puede no ser siempre precisa si la última página coincide
            //// exactamente con PageSize.
            //else // TotalCount == -1 (Keyset sin conteo)
            //{
            //    HasNextPage = Items.Count == this.PageSize && this.PageSize > 0;
            //}
        }
        /// <inheritdoc/>
        public bool HasNextPage { get;internal set; }

        /// <inheritdoc/>
        public bool HasPreviousPage => PageNumber > 1;

        /// <inheritdoc/>
        public object LastKeyValue { get; internal set; }

        /// <inheritdoc/>
        public int PageNumber { get; internal set; }

        /// <inheritdoc/>
        public int PageSize { get; internal set; }

        /// <inheritdoc/>
        public int TotalCount { get; internal set; }

        /// <inheritdoc/>
        public int TotalPages { get; internal set; }
    }
    /// <summary>
    /// Implementación concreta del resultado paginado.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de entidad.</typeparam>
    public class PagedResult<TEntity> : PagedResult, IPagedResult<TEntity>
    {
        #region Constructors

        /// <summary>
        /// Constructor para inicializar el resultado paginado.
        /// </summary>
        /// <param name="items">Los elementos de la página actual.</param>
        /// <param name="totalCount">
        /// El número total de elementos. Para Keyset, puede ser -1 si no se calcula.
        /// </param>
        /// <param name="pageNumber">El número de la página actual.</param>
        /// <param name="pageSize">El tamaño de la página.</param>
        /// <param name="lastKeyValue">El valor clave del último elemento para paginación Keyset.</param>
        /// <param name="hasNextPageOverride">
        /// Permite sobreescribir el cálculo de HasNextPage, útil para Keyset.
        /// </param>
        public PagedResult(IReadOnlyList<TEntity> items, int totalCount, int pageNumber, int pageSize, object lastKeyValue = null, bool? hasNextPageOverride = null)
            :base (totalCount, pageNumber, pageSize, lastKeyValue, hasNextPageOverride)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            
            if (totalCount == -1) // (Keyset sin conteo)
            {
                HasNextPage = Items.Count == this.PageSize && this.PageSize > 0;
            }
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public IReadOnlyList<TEntity> Items { get; }


        #endregion Properties
    }
}