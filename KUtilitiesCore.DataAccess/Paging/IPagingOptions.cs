namespace KUtilitiesCore.DataAccess.Paging
{
    /// <summary>
    /// Define las opciones para solicitar una página de datos, soportando diferentes estrategias.
    /// </summary>
    public interface IPagingOptions
    {

        /// <summary>
        /// El valor del último elemento de la página anterior, usado para PagingStrategy.Keyset. El
        /// tipo de este objeto debe coincidir con el tipo de la propiedad por la que se ordena.
        /// Para la primera página de Keyset, este valor puede ser null.
        /// </summary>
        object AfterValue { get; }

        /// <summary>
        /// Número de página solicitada (basado en 1). Usado principalmente para
        /// PagingStrategy.Offset. Para Keyset, puede ser ignorado o usado como referencia si
        /// AfterValue no se provee (para la primera página).
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Número de elementos por página.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Indica si se debe omitir la paginación por completo. Si es true, se ignorarán otros
        /// parámetros de paginación y se devolverán todos los resultados.
        /// </summary>
        bool SkipPagination { get; }

        /// <summary>
        /// Estrategia de paginación a utilizar. Por defecto es Offset.
        /// </summary>
        PagingStrategy Strategy { get; }

        // Opcional: Podría añadirse KeysetPropertyName si la inferencia desde
        //           ISpecification.OrderBy no es suficiente string KeysetPropertyName { get; }
    }
}