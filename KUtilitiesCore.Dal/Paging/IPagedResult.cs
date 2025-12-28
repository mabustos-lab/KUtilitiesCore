namespace KUtilitiesCore.Dal.Paging
{
    /// <summary>
    /// Representa el resultado de una consulta paginada.
    /// </summary>
    public interface IPagedResult
    {

        /// <summary>
        /// Obtiene un valor que indica si hay una página siguiente de resultados disponible.
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Obtiene un valor que indica si hay una página anterior de resultados disponible.
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// El valor de la propiedad de ordenación del último elemento en <see cref="Items"/>. Se
        /// utiliza para solicitar la siguiente página cuando se usa PagingStrategy.Keyset. Es null
        /// si Items está vacío o si la estrategia no es Keyset.
        /// </summary>
        /// <remarks>Para Keyset, la paginación hacia atrás es más compleja y no se aborda aquí.</remarks>
        object LastKeyValue { get; }

        /// <summary>
        /// Obtiene el número de página actual de este resultado.
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Obtiene el número de elementos por página
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Número total de elementos que coinciden con la consulta (sin paginar). Para
        /// PagingStrategy.Keyset, este valor puede ser -1 o no ser fiable si no se calcula
        /// explícitamente, ya que Keyset no requiere conocer el conteo total para funcionar.
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Número total de páginas disponibles. Para PagingStrategy.Keyset, este valor puede ser -1
        /// o no ser fiable.
        /// </summary>
        int TotalPages { get; }

    }

    /// <summary>
    /// Representa el resultado de una consulta paginada.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de entidad en la colección.</typeparam>
    public interface IPagedResult<out TEntity> : IPagedResult
    {

        /// <summary>
        /// Colleción de resultados de la página actual
        /// </summary>
        IReadOnlyList<TEntity> Items { get; }

    }
}