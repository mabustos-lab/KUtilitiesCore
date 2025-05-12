using System;
using System.Linq;

namespace KUtilitiesCore.DataAccess.Paging
{
    /// <summary>
    /// Implementación concreta simple de las opciones de paginación.
    /// </summary>
    public class PagingOptions : IPagingOptions
    {
        #region Fields

        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 100;
        private const int MaxPageSize = 1000; // Límite opcional para el tamaño de página

        private int _pageNumber = DefaultPageNumber;
        private int _pageSize = DefaultPageSize;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public PagingOptions()
        { }

        /// <summary>
        /// Constructor para inicializar las opciones de paginación.
        /// </summary>
        /// <param name="pageNumber">Número de página (basado en 1).</param>
        /// <param name="pageSize">Tamaño de la página.</param>
        public PagingOptions(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value > 0 ? value : DefaultPageNumber;
        }

        /// <inheritdoc/>
        public int PageSize
        {
            get => _pageSize;
            // Limita el tamaño máximo de página para evitar sobrecargas
            set => _pageSize = value > 0 ? Math.Min(value, MaxPageSize) : DefaultPageSize;
        }

        /// <inheritdoc/>
        public bool SkipPagination { get; set; } = false;

        #endregion Properties
    }
}