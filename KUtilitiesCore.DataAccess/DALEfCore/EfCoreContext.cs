#if NETFRAMEWORK
using System.Data.Entity;
#elif NETCOREAPP
using KUtilitiesCore;
using KUtilitiesCore.DataAccess.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
#endif

using System.Data;
using System.Data.Common;

namespace KUtilitiesCore.DataAccess.DALEfCore
{
    /// <summary>
    /// Implementación concreta de <see cref="IEfCoreContext"/> que proporciona acceso a
    /// funcionalidades de Entity Framework (EF6 y EF Core) junto con ejecución SQL.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa el patrón Adapter para exponer un DbContext de Entity Framework como
    /// un contexto compatible con las interfaces de acceso a datos unificadas.
    ///
    /// - Gestiona transacciones usando TransactionBase
    /// - Proporciona acceso directo al DbContext subyacente
    /// - Implementa ejecución SQL directa usando la conexión del contexto
    /// - Maneja correctamente la liberación de recursos
    /// </remarks>
    public sealed class EfCoreContext : IEfCoreContext
    {
        #region Fields

        private readonly DbContext _context;
        private bool _disposed;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia del contexto EF.
        /// </summary>
        /// <param name="context">Instancia de DbContext (EF6 o EF Core)</param>
        /// <exception cref="ArgumentNullException">Si context es nulo</exception>
        public EfCoreContext(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion Constructors

        #region IEfCoreContext Implementation

        /// <inheritdoc/>
        public DbContext Context => _context;

        #endregion IEfCoreContext Implementation

        #region ISqlExecutorContext Implementation

        /// <inheritdoc/>
        public DbConnection Connection
        {
            get
            {
#if NETFRAMEWORK
                // EF6: Acceso directo a la conexión
                return _context.Database.Connection;
#elif NETCOREAPP
                // EF Core: Obtiene la conexión a través de GetDbConnection()
                return _context.Database.GetDbConnection();
#endif
            }
        }

        /// <inheritdoc/>
        public IDaoParameterCollection CreateParameterCollection()
        {
            // Usa el proveedor de parámetros del contexto
            var factory = DbProviderFactories.GetFactory(Connection);
            return new DaoParameterCollection(() => factory.CreateParameter());
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery(
            string sql,
            IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text,
            ITransaction transaction = null)
        {
            using var command = CreateCommand(sql, parameters, commandType, transaction);
            return command.ExecuteNonQuery();
        }

        /// <inheritdoc/>
        public async Task<int> ExecuteNonQueryAsync(
            string sql,
            IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text,
            ITransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            using var command = CreateCommand(sql, parameters, commandType, transaction);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public TResult Scalar<TResult>(string sql, IDaoParameterCollection parameters = null)
        {
            using var command = CreateCommand(sql, parameters);
            var result = command.ExecuteScalar();
            return result is DBNull ? default : (TResult)result;
        }

        /// <inheritdoc/>
        public async Task<TResult> ScalarAsync<TResult>(
            string sql,
            IDaoParameterCollection parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var command = CreateCommand(sql, parameters);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is DBNull or null ? default : (TResult)result;
        }

        #endregion ISqlExecutorContext Implementation

        #region IDalContext Implementation

        /// <inheritdoc/>
        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
#if NETFRAMEWORK
            // EF6: BeginTransaction retorna DbContextTransaction
            var efTransaction = _context.Database.BeginTransaction(isolationLevel);
            return new TransactionEF(efTransaction);
#elif NETCOREAPP
            // EF Core: BeginTransaction retorna IDbContextTransaction
            IDbContextTransaction efTransaction = _context.Database.BeginTransaction();

            return new TransactionEF(efTransaction);
#endif
        }

        /// <inheritdoc/>
        public bool DatabaseExists()
        {
            try
            {
#if NET48
                // EF6
                return _context.Database.Exists();
#else
                // EF Core
                return _context.Database.CanConnect();
#endif
            }
            catch
            {
                return false;
            }
        }

        #endregion IDalContext Implementation

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion IDisposable Implementation

        #region Private Helpers

        /// <summary>
        /// Crea un comando de base de datos configurado correctamente.
        /// </summary>
        private DbCommand CreateCommand(
            string sql,
            IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text,
            ITransaction transaction = null)
        {
            var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = commandType;
            command.Transaction = transaction != null
                ? ((TransactionBase)transaction).GetTransactionObject()
                : null;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            return command;
        }

        #endregion Private Helpers
    }
}