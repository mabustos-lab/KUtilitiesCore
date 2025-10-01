using System.Collections;
using System.Data;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Convierte datos de un IDataReader en conjuntos de resultados fuertemente tipados.
    /// </summary>
    public sealed class DataReaderConverter : IDataReaderConverter, IReaderResultSet
    {
        #region Fields

        private readonly List<Func<IDataReader, IEnumerable>> _resultSets;
        private readonly ReaderResultSet readerResultSet;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor privado para inicializar el DataReaderConverter.
        /// </summary>
        private DataReaderConverter()
        {
            _resultSets = new List<Func<IDataReader, IEnumerable>>();
            readerResultSet = new ReaderResultSet();
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public bool HasResultsets => readerResultSet.HasResultsets;

        /// <summary>
        /// Indica si el convertidor puede realizar conversiones
        /// </summary>
        public bool RequiredConvert => _resultSets.Count > 0;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Método de fábrica para crear una nueva instancia de DataReaderConverter.
        /// </summary>
        /// <returns>Una instancia de IDataReaderConverter.</returns>
        public static IDataReaderConverter Create()
        {
            return new DataReaderConverter();
        }

        /// <inheritdoc/>
        IEnumerable<TResult> IReaderResultSet.GetResult<TResult>()
        {
            return readerResultSet.GetResult<TResult>();
        }

        /// <summary>
        /// Agrega una transformación de conjunto de resultados al convertidor.
        /// </summary>
        /// <typeparam name="TResult">El tipo del conjunto de resultados.</typeparam>
        /// <returns>La instancia actual de IDataReaderConverter.</returns>
        IDataReaderConverter IDataReaderConverter.WithResult<TResult>()
        {
            _resultSets.Add((reader) =>
            {
                try
                {
                    return reader.Translate<TResult>().ToList();
                }
                catch (Exception ex)
                {
                    ex.Data.Add("TResultType", typeof(TResult).FullName);
                    throw;
                }
            });
            return this;
        }

        /// <summary>
        /// Agrega un conjunto de resultados a la colección interna.
        /// </summary>
        /// <param name="resultSet">El conjunto de resultados a agregar.</param>
        internal void AddResultSet(IEnumerable resultSet)
        {
            readerResultSet.AddResult(resultSet);
        }

        /// <summary>
        /// Traduce los datos de un IDataReader en una colección de conjuntos de resultados
        /// fuertemente tipados.
        /// </summary>
        /// <param name="reader">
        /// El lector de datos que contiene los conjuntos de resultados a traducir.
        /// </param>
        /// <returns>
        /// Una instancia de IReaderResultSet que contiene los conjuntos de resultados traducidos.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Se lanza si el parámetro <paramref name="reader"/> es nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si no hay transformaciones definidas o si alguna transformación es nula.
        /// </exception>
        /// <exception cref="DataException">
        /// Se lanza si ocurre un error al procesar los datos del lector.
        /// </exception>
        internal IReaderResultSet Translate(IDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader), "El parámetro reader no puede ser nulo.");

            if (_resultSets == null || !_resultSets.Any())
                throw new InvalidOperationException("No hay transformaciones definidas para procesar el lector de datos.");

            try
            {
                foreach (var translate in _resultSets)
                {
                    if (translate == null)
                    {
                        throw new InvalidOperationException("Se encontró una transformación nula.");
                    }

                    readerResultSet.AddResult(translate(reader));

                    if (!reader.NextResult())
                    {
                        break; // Salir si no hay más conjuntos de resultados.
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataException("Ocurrió un error al traducir los datos del IDataReader.", ex);
            }

            return readerResultSet;
        }

        #endregion Methods
    }
}