using KUtilitiesCore.DataAccess.EfCore.Repositories;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;

namespace KUtilitiesCore.DataAccess.EfCore.UOW
{
    /// <summary>
    /// Implementación de Unit of Work para Entity Framework Core. Gestiona transacciones y la creación de repositorios.
    /// </summary>
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private Hashtable _repositories;
        private bool _disposed;

        public EfUnitOfWork(DbContext context) { _context = context; }
        
        /// <inheritdoc/>
        public IRepository<T> Repository<T>() where T : class
        {
            if(_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if(!_repositories.ContainsKey(type))
            {
                // Creamos una instancia del repositorio concreto inyectándole el contexto actual
                var repositoryType = typeof(EfRepository<T>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type]!;
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync() { return await _context.SaveChangesAsync(); }
        /// <inheritdoc/>
        public int SaveChanges() { return _context.SaveChanges(); }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                if(disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        
    }
}
