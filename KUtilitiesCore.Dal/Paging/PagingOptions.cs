namespace KUtilitiesCore.Dal.Paging
{
    /// <summary>
    /// Implementación concreta de las opciones de paginación.
    /// </summary>
    public class PagingOptions : IPagingOptions
    {
        #region Fields

        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        private int _pageNumber = DefaultPageNumber;
        private int _pageSize = DefaultPageSize;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor por defecto, usa PagingStrategy.Offset.
        /// </summary>
        public PagingOptions()
        { }

        /// <summary>
        /// Constructor para inicializar las opciones de paginación con estrategia Offset.
        /// </summary>
        /// <param name="pageNumber">Número de página (basado en 1).</param>
        /// <param name="pageSize">Tamaño de la página.</param>
        /// <param name="skipPagination">
        /// Indica si se debe omitir la paginación (opcional, por defecto false).
        /// </param>
        public PagingOptions(int pageNumber, int pageSize, bool skipPagination = false)
        {
            Strategy = PagingStrategy.Offset;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SkipPagination = skipPagination;
        }

        /// <summary>
        /// Constructor para inicializar las opciones de paginación con estrategia Keyset.
        /// </summary>
        /// <param name="pageSize">Tamaño de la página.</param>
        /// <param name="afterValue">
        /// El valor del último elemento de la página anterior (null para la primera página).
        /// </param>
        /// <param name="skipPagination">
        /// Indica si se debe omitir la paginación (opcional, por defecto false).
        /// </param>
        public PagingOptions(int pageSize, object afterValue, bool skipPagination = false)
        {
            Strategy = PagingStrategy.Keyset;
            PageSize = pageSize;
            AfterValue = afterValue;
            SkipPagination = skipPagination;
            PageNumber = 1; // Para Keyset, PageNumber es menos relevante, se puede setear a 1 o ignorar.
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public object AfterValue { get; set; } = null;

        /// <inheritdoc/>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value > 0) ? value : DefaultPageNumber;
        }

        /// <inheritdoc/>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > 0) ? Math.Min(value, MaxPageSize) : DefaultPageSize;
        }

        /// <inheritdoc/>
        public bool SkipPagination { get; set; } = false;

        /// <inheritdoc/>
        public PagingStrategy Strategy { get; set; } = PagingStrategy.Offset;

        #endregion Properties
    }
}