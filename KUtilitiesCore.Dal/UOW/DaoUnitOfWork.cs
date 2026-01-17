using System;
using System.Collections;
using System.Threading.Tasks;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using KUtilitiesCore.Dal;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Implementación de Unit of Work para KUtilitiesCore.Dal.
    /// Gestiona transacciones SQL nativas a través de IDAOContext.
    /// </summary>
    public class DaoUnitOfWork : IUnitOfWork
    {
        private readonly IDaoContext _context;
        private Hashtable _repositories;
        private bool _disposed;
        private ITransaction _currentTransaction;

        /// <summary>
        /// Inicia una transacción explícita. 
        /// Las operaciones subsiguientes de los repositorios deben usar este contexto transaccional.
        /// </summary>
        protected ITransaction Transaction
        {
            get
            {
                _currentTransaction = _currentTransaction ?? _context.BeginTransaction();
                return _currentTransaction;
            }
        }

        public DaoUnitOfWork(IDaoContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                // Aquí creamos una instancia de DaoRepository (o un derivado registrado).
                // Nota: Si usas repositorios específicos (ej. ProductRepository : DaoRepository<Product>),
                // necesitarías un mecanismo de Factory o DI más complejo aquí.
                // Por defecto, usamos el genérico.

                var repositoryType = typeof(DaoRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), GetUowContext());

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }
        /// <summary>
        /// En el contexto de ADO.NET/Dal, SaveChanges generalmente confirma la transacción abierta.
        /// Si no hay transacción explícita y las operaciones fueron atómicas, no hace nada.
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            // Simulación asíncrona ya que IDAOContext suele ser síncrono en commits
            return await Task.Run(() => SaveChanges());
        }
        /// <inheritdoc/>
        public int SaveChanges()
        {
            try
            {
                if (_currentTransaction!=null)
                {
                    Transaction.Commit();
                    _currentTransaction = null; // Limpiamos tras commit
                    return 1; // Éxito
                }
                return 0; // Nada que confirmar
            }
            catch
            {
                Rollback();
                throw;
            }
        }


        private IDaoUowContext GetUowContext() => new DaoUowContext(() => _context, () => Transaction);

        void Rollback()
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Rollback();
                _currentTransaction = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Liberamos la transacción si quedó pendiente
                    if (_currentTransaction != null)
                    {
                        _currentTransaction.Rollback();
                        _currentTransaction.Dispose();
                    }

                    // Nota: No disponemos _context aquí si fue inyectado (DI), 
                    // a menos que UOW sea el dueño absoluto de su ciclo de vida.
                    _context.Dispose(); 
                }
            }
            _disposed = true;
        }
    }
}
