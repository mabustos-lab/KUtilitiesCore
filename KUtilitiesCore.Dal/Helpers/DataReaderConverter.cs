using System.Collections;
using System.Data;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Convierte datos de un IDataReader en conjuntos de resultados fuertemente tipados.
    /// </summary>
    public sealed class DataReaderConverter : IDataReaderConverter, IReaderResultSet
    {
        private readonly List<Func<IDataReader, IEnumerable>> _resultSets;
        private readonly ReaderResultSet _readerResultSet;
        private bool _useDefaultDataTable;
        private readonly TranslateOptions _translateOptions;

        private DataReaderConverter()
        {
            _resultSets = new List<Func<IDataReader, IEnumerable>>();
            _readerResultSet = new ReaderResultSet();
            _useDefaultDataTable = false;
            _translateOptions = new TranslateOptions();
        }
        /// <inheritdoc/>
        public bool HasResultsets => _readerResultSet.HasResultsets;
        /// <inheritdoc/>
        public int ResultSetCount => _readerResultSet.ResultSetCount;
        /// <inheritdoc/>
        public bool RequiredConvert => _resultSets.Count > 0 || _useDefaultDataTable;

        public static IDataReaderConverter Create()
        {
            return new DataReaderConverter();
        }
        /// <inheritdoc/>
        public IDataReaderConverter WithResult<TResult>() where TResult : class, new()
        {
            _useDefaultDataTable = false;

            // Pre-compilar las propiedades requeridas para StrictMapping
            string[] requiredProperties = Array.Empty<string>();
            if (_translateOptions.StrictMapping)
            {
                requiredProperties = IDataReaderExt.GetPropertiesRequired<TResult>();
            }

            _resultSets.Add(reader =>
            {
                try
                {
                    _translateOptions.RequiredProperties = requiredProperties;
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
        /// <inheritdoc/>
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
                        var transform = _resultSets[resultSetIndex];
                        resultSet = transform(reader);
                    }

                    _readerResultSet.AddResult(resultSet);
                    resultSetIndex++;
                }
                while (reader.NextResult());

                return this;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DataException("Ocurrió un error al traducir los datos del IDataReader.", ex);
            }
        }

        private static DataTable ConvertToDataTable(IDataReader reader)
        {
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        /// <summary>
        /// Obtiene el ResulSet
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="index"></param>
        /// <returns>Regresa el ResulSet mapeado en un tipo especifico</returns>
        public IEnumerable<TResult> GetResult<TResult>(int index = 0) where TResult : class, new()
        {
            return _readerResultSet.GetResult<TResult>(index);
        }
        /// <inheritdoc/>
        public DataTable GetDataTable(int index = 0)
        {
            return _readerResultSet.GetDataTable(index);
        }
        /// <inheritdoc/>
        public DataTable[] GetAllDataTables()
        {
            return _readerResultSet.GetAllDataTables();
        }
        /// <inheritdoc/>
        public IDataReaderConverter SetStrictMapping(bool value)
        {
            _translateOptions.StrictMapping = value;
            return this;
        }
        /// <inheritdoc/>
        public IDataReaderConverter SetIgnoreMissingColumns(bool value)
        {
            _translateOptions.IgnoreMissingColumns = value;
            return this;
        }
        /// <inheritdoc/>
        public IDataReaderConverter SetColumnPrefixesToRemove(params string[] args)
        {
            _translateOptions.ColumnPrefixesToRemove = args;
            return this;
        }
    }
}