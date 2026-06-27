#nullable enable
using KUtilitiesCore.Extensions;
using System.Collections;
using System.Data;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Strategy for mapping an IDataReader's current result set to a DataTable.
    /// </summary>
    internal class DataTableMappingStrategy : IMappingStrategy
    {
        public object Map(DataTable dataTable)
        {
            return dataTable;
        }
    }

    /// <summary>
    /// Strategy for mapping an IDataReader's current result set to a List of a specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to which the data reader's results should be mapped.</typeparam>
    internal class ObjectMappingStrategy<TResult> : IMappingStrategy where TResult : class, new()
    {
        private readonly TranslateOptions _options;

        public ObjectMappingStrategy(TranslateOptions options = null)
        {
            _options = options ?? new TranslateOptions();
        }

        public object Map(DataTable dataTable)
        {
            // Reusing the DataTableToEnumerable logic from ReaderResultSet
            return ReaderResultSet.DataTableToEnumerable<TResult>(dataTable, _options).ToList();
        }
    }

    /// <summary>
    /// Estrategia que permite mapear un DataTable a una colección de objetos mediante un delegado personalizado.
    /// Esta estrategia no impone restricciones de tipo, permitiendo la creación de structs, records o cualquier
    /// tipo de objeto sin necesidad de un constructor sin parámetros.
    /// </summary>
    /// <typeparam name="TResult">El tipo de objeto al que se mapeará cada fila del DataTable.</typeparam>
    internal class DelegateMappingStrategy<TResult> : IMappingStrategy
    {
        private readonly Func<DataTable, IEnumerable<TResult>> _mapper;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DelegateMappingStrategy{TResult}"/> con el delegado de mapeo especificado.
        /// </summary>
        /// <param name="mapper">Función que recibe un DataTable y retorna una colección de objetos del tipo <typeparamref name="TResult"/>.</param>
        public DelegateMappingStrategy(Func<DataTable, IEnumerable<TResult>> mapper)
        {
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public object Map(DataTable dataTable)
        {
            return _mapper(dataTable).ToList();
        }
    }
}
