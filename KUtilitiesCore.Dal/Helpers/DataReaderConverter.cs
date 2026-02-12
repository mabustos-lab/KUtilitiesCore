using KUtilitiesCore.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Convierte datos de un IDataReader en conjuntos de resultados fuertemente tipados.
    /// </summary>
    public sealed class DataReaderConverter : IDataReaderConverter
    {
        #region Fields

        private readonly Queue<IMappingStrategy> _mappingStrategies;
        private readonly TranslateOptions _translateOptions;
        private bool _useDefaultDataTable;
        private IDaoParameterCollection _parametersUsed; // To store parameters to be passed to ReaderResultSet

        #endregion Fields

        #region Constructors

        private DataReaderConverter()
        {
            _mappingStrategies = new Queue<IMappingStrategy>();
            _useDefaultDataTable = false;
            _translateOptions = new TranslateOptions();
        }

        #endregion Constructors



        #region Methods

        public static IDataReaderConverter Create()
        {
            return new DataReaderConverter();
        }

        public static IDataReaderConverter GetDefault()
        {
            var converter = new DataReaderConverter();
            converter.WithDefaultDataTable();
            return converter;
        }

        /// <inheritdoc/>
        public IDataReaderConverter SetColumnPrefixesToRemove(params string[] args)
        {
            _translateOptions.ColumnPrefixesToRemove = args;
            return this;
        }

        /// <inheritdoc/>
        public IDataReaderConverter SetStrictMapping(bool value)
        {
            _translateOptions.StrictMapping = value;
            return this;
        }

        /// <inheritdoc/>
        public IDataReaderConverter WithDefaultDataTable()
        {
            _useDefaultDataTable = true;
            return this;
        }

        /// <inheritdoc/>
        public IDataReaderConverter WithResult<TResult>() where TResult : class, new()
        {
            _mappingStrategies.Enqueue(new ObjectMappingStrategy<TResult>(_translateOptions));
            _useDefaultDataTable = false; // If specific results are configured, default table is not used unless explicitly set again for subsequent results.
            return this;
        }

        /// <summary>
        /// Sets the parameters used for the query, to be passed to the ReaderResultSet.
        /// </summary>
        /// <param name="parameters">The collection of parameters.</param>
        internal void SetParametersUsed(IDaoParameterCollection parameters = null)
        {
            _parametersUsed = parameters;
        }

        /// <inheritdoc/>
        public IReaderResultSet BuildReaderResultSet(IDataReader reader, ref bool moreResultSets)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader), "El parámetro reader no puede ser nulo.");

            var readerResultSet = new ReaderResultSet();
            try
            {
                readerResultSet.Load(reader, _mappingStrategies, _useDefaultDataTable); // Load processes only the current result set
                readerResultSet.SetParams(_parametersUsed); // Pass the parameters to the ReaderResultSet
                moreResultSets = reader.NextResult(); // Check for more result sets after processing the current one
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