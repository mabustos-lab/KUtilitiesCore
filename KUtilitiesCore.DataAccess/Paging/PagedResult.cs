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
        /// <summary>
        /// Obtiene la lista de solo lectura de elementos para la página actual.
        /// </summary>
        public IReadOnlyList<TEntity> Items { get; }

        /// <summary>
        /// Obtiene el número de la página actual (basado en 1).
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Obtiene el tamaño de página solicitado.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Obtiene el número total de elementos que coinciden con la consulta.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Obtiene el número total de páginas disponibles.
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Obtiene un valor que indica si existe una página anterior.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Obtiene un valor que indica si existe una página siguiente.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Constructor para inicializar el resultado paginado.
        /// </summary>
        /// <param name="items">Los elementos de la página actual (como lista de solo lectura).</param>
        /// <param name="totalCount">El número total de elementos que coinciden con la consulta.</param>
        /// <param name="pageNumber">El número de la página actual.</param>
        /// <param name="pageSize">El tamaño de la página.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="items"/> es null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Si <paramref name="pageNumber"/> o <paramref name="pageSize"/> no son positivos.</exception>
        public PagedResult(IReadOnlyList<TEntity> items, int totalCount, int pageNumber, int pageSize)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "El número de página debe ser mayor que 0.");
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe ser mayor que 0.");

            // IReadOnlyList<T> ya proporciona la inmutabilidad deseada.
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            // Calcula el número total de páginas. Ceiling asegura que incluso una fracción de página cuente como una página completa.
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        /// <summary>
        /// Constructor alternativo que acepta IList<T> y lo convierte internamente.
        /// Útil si la fuente de datos devuelve IList<T> pero quieres exponer IReadOnlyList<T>.
        /// </summary>
        /// <param name="items">Los elementos de la página actual (como lista modificable).</param>
        /// <param name="totalCount">El número total de elementos que coinciden con la consulta.</param>
        /// <param name="pageNumber">El número de la página actual.</param>
        /// <param name="pageSize">El tamaño de la página.</param>
        public PagedResult(IList<TEntity> items, int totalCount, int pageNumber, int pageSize)
            : this(items as IReadOnlyList<TEntity> ?? new List<TEntity>(items).AsReadOnly(), totalCount, pageNumber, pageSize)
        {
            // La lógica principal está en el otro constructor.
            // Se convierte IList<T> a IReadOnlyList<T>. Si ya es IReadOnlyList<T>, se usa directamente.
            // Si no, se crea una copia en un List<T> y se expone su wrapper AsReadOnly().
        }
    }
}
