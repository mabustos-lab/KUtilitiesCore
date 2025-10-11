namespace KUtilitiesCore.DataAccess.Paging
{
    /// <summary>
    /// Representa el resultado de una consulta paginada.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de entidad en la colección.</typeparam>
    public interface IPagedResult<out TEntity>
    {
        #region Properties

        /// <inheritdoc/>
        bool HasNextPage { get; }

        /// <inheritdoc/>
        bool HasPreviousPage { get; }

        /// <inheritdoc/>
        IReadOnlyList<TEntity> Items { get; }

        // Para Keyset, la paginación hacia atrás es más compleja y no se aborda aquí.
        /// <summary>
        /// El valor de la propiedad de ordenación del último elemento en <see cref="Items"/>. Se
        /// utiliza para solicitar la siguiente página cuando se usa PagingStrategy.Keyset. Es null
        /// si Items está vacío o si la estrategia no es Keyset.
        /// </summary>
        object LastKeyValue { get; }

        /// <inheritdoc/>
        int PageNumber { get; }

        /// <inheritdoc/>
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

        #endregion Properties
    }
}