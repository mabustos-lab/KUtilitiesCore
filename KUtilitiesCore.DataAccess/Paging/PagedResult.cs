using System;
using System.Linq;

namespace KUtilitiesCore.DataAccess.Paging
{
    /// <summary>
    /// Implementación concreta del resultado paginado.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de entidad.</typeparam>
    public class PagedResult<TEntity> : IPagedResult<TEntity>
    {
        /// <inheritdoc/>
        public IReadOnlyList<TEntity> Items { get; }
        /// <inheritdoc/>
        public int PageNumber { get; }
        /// <inheritdoc/>
        public int PageSize { get; }
        /// <inheritdoc/>
        public int TotalCount { get; }
        /// <inheritdoc/>
        public int TotalPages { get; }
        /// <inheritdoc/>
        public object LastKeyValue { get; }

        /// <inheritdoc/>
        public bool HasPreviousPage => PageNumber > 1;

        /// <inheritdoc/>
        public bool HasNextPage { get; }


        /// <summary>
        /// Constructor para inicializar el resultado paginado.
        /// </summary>
        /// <param name="items">Los elementos de la página actual.</param>
        /// <param name="totalCount">El número total de elementos. Para Keyset, puede ser -1 si no se calcula.</param>
        /// <param name="pageNumber">El número de la página actual.</param>
        /// <param name="pageSize">El tamaño de la página.</param>
        /// <param name="lastKeyValue">El valor clave del último elemento para paginación Keyset.</param>
        /// <param name="hasNextPageOverride">Permite sobreescribir el cálculo de HasNextPage, útil para Keyset.</param>
        public PagedResult(IReadOnlyList<TEntity> items, int totalCount, int pageNumber, int pageSize, object lastKeyValue = null, bool? hasNextPageOverride = null)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "El número de página debe ser mayor que 0.");
            // Si pageSize es 0 y totalCount > 0, es una situación anómala si no se está omitiendo la paginación
            // y hay elementos. Si se omite la paginación, pageSize se ajusta a totalCount.
            if (pageSize <= 0 && totalCount > 0 && items.Any())
            {
                // Esta validación podría ser demasiado estricta si SkipPagination=true y pageSize no se ajustó correctamente antes.
                // throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe ser mayor que 0 si hay elementos y no se omite la paginación.");
            }


            Items = items;
            TotalCount = totalCount; // Para Keyset, esto podría ser el recuento de la página actual si no se conoce el total.
            PageNumber = pageNumber;
            // Si pageSize es 0 (y totalCount es 0), o si pageSize es igual a totalCount (caso de SkipPagination),
            // TotalPages es 1 (o 0 si no hay items).
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

            // HasNextPage:
            // Si se proporciona un override (útil para Keyset donde se sabe si hay más por el número de items devueltos vs pageSize)
            if (hasNextPageOverride.HasValue)
            {
                HasNextPage = hasNextPageOverride.Value;
            }
            // Para Offset pagination, o si TotalCount es conocido
            else if (TotalCount >= 0)
            {
                HasNextPage = PageNumber < TotalPages;
            }
            // Para Keyset sin TotalCount, si Items.Count == PageSize, es probable que haya más.
            // Esto es una heurística y puede no ser siempre precisa si la última página coincide exactamente con PageSize.
            else // TotalCount == -1 (Keyset sin conteo)
            {
                HasNextPage = Items.Count == this.PageSize && this.PageSize > 0;
            }
        }

        /// <summary>
        /// Constructor alternativo que acepta IList<T> y lo convierte internamente.
        /// </summary>
        public PagedResult(IList<TEntity> items, int totalCount, int pageNumber, int pageSize, object lastKeyValue = null, bool? hasNextPageOverride = null)
            : this(items as IReadOnlyList<TEntity> ?? new List<TEntity>(items).AsReadOnly(), totalCount, pageNumber, pageSize, lastKeyValue, hasNextPageOverride)
        {
        }
    }
}
