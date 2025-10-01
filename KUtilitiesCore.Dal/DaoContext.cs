
using KUtilitiesCore.Dal.ConnectionBuilder;
using KUtilitiesCore.Dal.Helpers;
using System.Data;
using System.Data.Common;

namespace KUtilitiesCore.Dal
{
    /// <summary>
    /// Proporciona un contexto para las operaciones de acceso a datos, incluyendo la creación de
    /// comandos, transacciones y gestión de conexiones a la base de datos.
    /// </summary>
    /// <remarks>
    /// Esta clase sirve como punto central para ejecutar operaciones de base de datos, como
    /// ejecutar consultas, gestionar transacciones e interactuar con conjuntos de datos. Abstrae el
    /// proveedor de base de datos subyacente y asegura patrones de acceso consistentes.
    /// </remarks>
    public class DaoContext : IDaoContext
    {
        #region Fields

        private readonly IConnectionString _connectionString;
        private readonly DbProviderFactory _factory;
        private Lazy<DbConnection> _connection;
        private bool _disposedValue;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DaoContext"/> con la conexión
        /// especificada constructor.
        /// </summary>
        /// <param name="cnnStr">
        /// El constructor de conexión que proporciona la cadena de conexión y el nombre del
        /// proveedor para el acceso a la base de datos. Este parámetro no puede ser <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="cnnStr"/> is <see langword="null"/>.
        /// </exception>
        /// .
        public DaoContext(IConnectionString cnnStr)
        {
            _connectionString = cnnStr ?? throw new ArgumentNullException(nameof(cnnStr));
            _factory = DbProviderFactories.GetFactory(cnnStr.ProviderName);
            _connection = new Lazy<DbConnection>(CreateConnection);
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public DbConnection Connection
            => _connection.Value;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            return new TransactionBase(
                Connection.BeginTransaction(isolationLevel));
        }

        /// <inheritdoc/>
        public DbDataAdapter CreateAdapter(DbCommand command)
        {
            DbDataAdapter dataAdapter = _factory.CreateDataAdapter();
            dataAdapter.SelectCommand = command;
            return dataAdapter;
        }

        /// <inheritdoc/>
        public DbCommand CreateCommand(string sql,
            IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text, ITransaction transaction = null)
        {
            DbCommand command = _factory.CreateCommand();
            if (transaction != null) command.Transaction = ((TransactionBase)transaction).GetTransactionObject();
            command.Connection = Connection;
            command.CommandText = sql;
            command.CommandType = commandType;
            if (parameters != null)
            {
                foreach (DbParameter p in parameters)
                {
                    command.Parameters.Add(p);
                }
            }
            return command;
        }

        /// <inheritdoc/>
        public DbCommandBuilder CreateCommandBuilder()
        {
            DbCommandBuilder cmdBuilder = _factory.CreateCommandBuilder();
            return cmdBuilder;
        }

        /// <inheritdoc/>
        public IDaoParameterCollection CreateParameterCollection()
        {
            return new DaoParameterCollection(CreateParameter);
        }

        /// <inheritdoc/>
        public bool DatabaseExists()
        {
            if (_connection.IsValueCreated && _connection.Value.State == ConnectionState.Open)
                return true;

            try
            {
                using (var tempCnn = CreateConnection())
                {
                    return tempCnn.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery(string sql, IDaoParameterCollection parameters = null,
                    CommandType commandType = CommandType.Text, ITransaction transaction = null)
        {
            using DbCommand command = CreateCommand(sql, parameters, commandType, transaction);
            return command.ExecuteNonQuery();
        }

        /// <inheritdoc/>
        public Task<int> ExecuteNonQueryAsync(string sql, IDaoParameterCollection parameters = null,
                    CommandType commandType = CommandType.Text, ITransaction transaction = null,
                    CancellationToken cancellationToken = default)
        {
            using DbCommand command = CreateCommand(sql, parameters, commandType, transaction);
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public IReaderResultSet ExecuteReader(string sql,
            IDataReaderConverter translate, IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.StoredProcedure)
        {
            translate ??= DataReaderConverter.Create();
            using DbCommand command = CreateCommand(sql, parameters, commandType);
            using DbDataReader reader = command.ExecuteReader();

            return translate.RequiredConvert
                ? ((DataReaderConverter)translate).Translate(reader)
                : new ReaderResultSet();
        }
        /// <inheritdoc/>
        public DataSet ExecuteReader(string sql,
           IDaoParameterCollection parameters = null,
           CommandType commandType = CommandType.StoredProcedure)
        {
            
            using DbCommand command = CreateCommand(sql, parameters, commandType);
            using DbDataAdapter adapter= CreateAdapter(command);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);

            return dataSet;
        }

        /// <inheritdoc/>
        public async Task<IReaderResultSet> ExecuteReaderAsync(string sql,
            IDataReaderConverter translate, IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default)
        {
            IReaderResultSet ret = new ReaderResultSet();
            translate ??= DataReaderConverter.Create();
            using (DbCommand command = CreateCommand(sql, parameters, commandType))
            {
                using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (translate.RequiredConvert)
                {
                    DataReaderConverter converter = (DataReaderConverter)translate;
                    ret = converter.Translate(reader);
                }
            }
            return ret;
        }

        /// <inheritdoc/>
        public void FillDataSet(string sql, string srcTable, DataSet ds, IDaoParameterCollection parameters = null)
        {
            using DbCommand command = CreateCommand(sql, parameters);
            using DbDataAdapter adapter = _factory.CreateDataAdapter();
            adapter.SelectCommand = command;
            adapter.Fill(ds, srcTable);
        }

        /// <inheritdoc/>
        public TResult Scalar<TResult>(string sql, IDaoParameterCollection parameters = null)
        {
            using DbCommand command = CreateCommand(sql, parameters);
            return (TResult)command.ExecuteScalar();
        }

        /// <inheritdoc/>
        public async Task<TResult> ScalarAsync<TResult>(string sql, IDaoParameterCollection parameters = null,
            CancellationToken cancellationToken = default)
        {
            using DbCommand command = CreateCommand(sql, parameters);
            object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result is DBNull or null ? default : (TResult)result;
        }

        /// <inheritdoc/>
        public int UpdateDataSet(DataSet ds, string selectCommandText, string tableName, ITransaction transaction = null)
        {
            using DbCommand command = CreateCommand(selectCommandText, null, CommandType.Text, transaction);
            using DbDataAdapter adapter = _factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            using DbCommandBuilder builder = CreateCommandBuilder();
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.UpdateCommand = builder.GetUpdateCommand();
            adapter.DeleteCommand = builder.GetDeleteCommand();

            // Asignar transacción a los comandos generados
            if (transaction != null)
            {
                var tx = ((TransactionBase)transaction).GetTransactionObject();
                if (adapter.InsertCommand != null) adapter.InsertCommand.Transaction = tx;
                if (adapter.UpdateCommand != null) adapter.UpdateCommand.Transaction = tx;
                if (adapter.DeleteCommand != null) adapter.DeleteCommand.Transaction = tx;
            }

            return adapter.Update(ds, tableName);
        }

        internal DbDataAdapter CreateDataAdapter()
        {
            return _factory.CreateDataAdapter();
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing && _connection != null && _connection.IsValueCreated)
                {
                    _connection.Value.Dispose();
                }
                _connection = null;
                _disposedValue = true;
            }
        }

        private DbConnection CreateConnection()
        {
            DbConnection cnn = _factory.CreateConnection();
            cnn.ConnectionString = _connectionString.CnnString;
            cnn.Open();
            return cnn;
        }

        private DbParameter CreateParameter()
                            => _factory.CreateParameter();

        #endregion Methods
    }
}