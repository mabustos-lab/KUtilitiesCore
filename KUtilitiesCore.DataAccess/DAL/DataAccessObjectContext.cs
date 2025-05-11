using KUtilitiesCore.DataAccess.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.DAL
{
    public class DataAccessObjectContext : IDAOContext
    {

        private Lazy<DbConnection> connection;
        private readonly IConnectionBuilder connectionString;
        private bool disposedValue;
        private readonly DbProviderFactory factory;

        public DataAccessObjectContext(IConnectionBuilder cnnStr)
        {
            connectionString = cnnStr;
            factory = DbProviderFactories.GetFactory(cnnStr.ProviderName);
            connection = new Lazy<DbConnection>(CreateConnection);
        }

        public DbConnection Connection
            => connection.Value;

        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            return new TransactionBase(
                Connection.BeginTransaction(isolationLevel));
        }

        public DbDataAdapter CreateAdapter(DbCommand command)
        {
            DbDataAdapter dataAdapter = factory.CreateDataAdapter();
            dataAdapter.SelectCommand = command;
            return dataAdapter;
        }

        public DbCommandBuilder CreateCommandBuilder()
        {
            DbCommandBuilder cmdBuilder = factory.CreateCommandBuilder();
            return cmdBuilder;
        }

        public DbCommand CreateCommand(string sql,
            IDbParameterCollection parameters = null,
            CommandType commandType = CommandType.Text, ITransaction transaction = null)
        {
            DbCommand command = factory.CreateCommand();
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

        public IDbParameterCollection CreateParameterCollection()
        {
            return new DbParameterCollection(CreateParameter);
        }

        public bool DatabaseExists()
        {
            if (connection.IsValueCreated && connection.Value.State == ConnectionState.Open) return true;
            try
            {
                using var _ = CreateConnection(); // Replace '_' with a discard '_'
                return _.State== ConnectionState.Open;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public int ExecuteNonQuery(string sql, IDbParameterCollection parameters = null,
                    CommandType commandType = CommandType.Text, ITransaction transaction = null)
        {
            using DbCommand command = CreateCommand(sql, parameters, commandType, transaction);
            return command.ExecuteNonQuery();
        }

        public IReaderResultSet ExecuteReader(string sql,
            IDataReaderConverter translate, IDbParameterCollection parameters = null,
            CommandType commandType = CommandType.StoredProcedure)
        {
            IReaderResultSet ret = new ReaderResultSet();
            translate ??= DataReaderConverter.Create();
            using (DbCommand command = CreateCommand(sql, parameters, commandType))
            {
                using DbDataReader reader = command.ExecuteReader();
                if (translate.RequiredConvert)
                {
                    DataReaderConverter converter = (DataReaderConverter)translate;
                    ret = converter.Translate(reader);
                }
            }
            return ret;
        }
     
        public TResult Scalar<TResult>(string sql, IDbParameterCollection parameters = null)
        {
            using DbCommand command = CreateCommand(sql, parameters);
            return (TResult)command.ExecuteScalar();
        }

        internal DbDataAdapter CreateDataAdapter()
        {
            return factory.CreateDataAdapter();
        }

        DbParameter CreateParameter()
        {
            return factory.CreateParameter();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && connection.IsValueCreated)
                {
                    connection.Value.Dispose();
                }
                connection = null;
                disposedValue = true;
            }
        }

        private DbConnection CreateConnection()
        {
            DbConnection cnn = factory.CreateConnection();
            cnn.ConnectionString = connectionString.CnnString;
            cnn.Open();
            return cnn;
        }

        public void FillDataSet(string sql,string srcTable,DataSet ds, IDbParameterCollection parameters = null)
        {
            using DbCommand command = CreateCommand(sql, parameters);
            using DbDataAdapter adapter = CreateDataAdapter();
            adapter.SelectCommand = command;
            adapter.Fill(ds, srcTable);
        }

        public int UpdateDataSet(DataSet ds, string selectCommandText, string tableName, ITransaction transaction = null)
        {
            using DbCommand command = CreateCommand(selectCommandText, null, CommandType.Text, transaction);
            using DbDataAdapter adapter = CreateDataAdapter();
            adapter.SelectCommand = command;
            // Se emplea DbCommandBuilder para generar InsertCommand, UpdateCommand y DeleteCommand
            using DbCommandBuilder builder = CreateCommandBuilder();
            adapter.UpdateBatchSize = 1000;
            builder.DataAdapter = adapter;
            // El método Update aplica los cambios hechos en el DataSet a la base de datos
            int affected = adapter.Update(ds, tableName);

            return affected;
        }
        

    }
}