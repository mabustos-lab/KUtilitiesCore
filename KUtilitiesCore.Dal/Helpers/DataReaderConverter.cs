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
        private readonly ReaderResultSet _readerResultSet;
        private bool _useDefaultDataTable;
        private readonly TranslateOptions _translateOptions;

        #endregion Fields

        #region Constructors

        private DataReaderConverter()
        {
            _resultSets = new List<Func<IDataReader, IEnumerable>>();
            _readerResultSet = new ReaderResultSet();
            _useDefaultDataTable = false;
            _translateOptions = new TranslateOptions();
        }

        #endregion Constructors

        #region Properties

        public bool HasResultsets => _readerResultSet.HasResultsets;
        public int ResultSetCount => _readerResultSet.ResultSetCount;
        public bool RequiredConvert => _resultSets.Count > 0 || _useDefaultDataTable;

        #endregion Properties

        #region Methods

        public static IDataReaderConverter Create()
        {
            return new DataReaderConverter();
        }

        public IDataReaderConverter WithResult<TResult>() where TResult : class, new()
        {
            _useDefaultDataTable = false;
            _resultSets.Add((reader) =>
            {
                try
                {
                    _translateOptions.RequiredProperties = [];
                    if (_translateOptions.StrictMapping)
                    {
                        _translateOptions.RequiredProperties = IDataReaderExt.GetPropertiesRequired<TResult>();
                    }
                    return reader.Translate<TResult>(_translateOptions).ToList();
                }
                catch (Exception ex)
                {
                    ex.Data.Add("TResultType", typeof(TResult).FullName);
                    throw;
                }
            });
            return this;
        }

        public IDataReaderConverter WithDefaultDataTable()
        {
            _useDefaultDataTable = true;
            return this;
        }

        internal IReaderResultSet Translate(IDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader), "El parámetro reader no puede ser nulo.");

            try
            {
                int resultSetIndex = 0;

                do
                {
                    IEnumerable resultSet;

                    if (_useDefaultDataTable || resultSetIndex >= _resultSets.Count)
                    {
                        // Comportamiento por defecto: convertir a DataTable
                        resultSet = (IEnumerable)ConvertToDataTable(reader);
                    }
                    else
                    {
                        // Usar transformación específica
                        var transform = _resultSets[resultSetIndex];
                        if (transform == null)
                            throw new InvalidOperationException($"Se encontró una transformación nula en el índice {resultSetIndex}.");

                        resultSet = transform(reader);
                    }

                    _readerResultSet.AddResult(resultSet);
                    resultSetIndex++;

                } while (reader.NextResult());

                return this;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new DataException("Ocurrió un error al traducir los datos del IDataReader.", ex);
            }
        }

        private DataTable ConvertToDataTable(IDataReader reader)
        {
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        public IEnumerable<TResult> GetResult<TResult>(int index = 0) where TResult : class, new()
        {
            return _readerResultSet.GetResult<TResult>(index);
        }

        public DataTable GetDataTable(int index = 0)
        {
            return _readerResultSet.GetDataTable(index);
        }

        public DataTable[] GetAllDataTables()
        {
            return _readerResultSet.GetAllDataTables();
        }
        #endregion Methods
        /// <summary>
        /// Si es true, lanza excepción cuando no se pueden mapear todas las propiedades requeridas
        /// </summary>
        public IDataReaderConverter SetStrictMapping(bool value)
        {
            _translateOptions.StrictMapping = value;
            return this;
        }
        /// <summary>
        /// Si es true, ignora propiedades que no existen en el resultado del reader
        /// </summary>
        public IDataReaderConverter SetIgnoreMissingColumns(bool value)
        {
            _translateOptions.IgnoreMissingColumns = value;
            return this;
        }
        /// <summary>
        /// Prefijos a remover de los nombres de columna antes del mapeo
        /// </summary>
        public IDataReaderConverter SetColumnPrefixesToRemove(params string[] args)
        {
            _translateOptions.ColumnPrefixesToRemove = args;
            return this;
        }
    }
}