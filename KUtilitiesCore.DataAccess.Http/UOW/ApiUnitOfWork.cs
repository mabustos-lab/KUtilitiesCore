using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using KUtilitiesCore.DataAccess.Http.Repositories;

namespace KUtilitiesCore.DataAccess.Http.UOW
{
    public class ApiUnitOfWork : IUnitOfWork
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _clientName;
        private Hashtable _repositories;
        private bool _disposed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpClientFactory">Fábrica de clientes HTTP de .NET Core.</param>
        /// <param name="clientName">Nombre del cliente configurado en Startup (con BaseAddress, Headers, etc).</param>
        public ApiUnitOfWork(IHttpClientFactory httpClientFactory, string clientName = "DefaultApi")
        {
            _httpClientFactory = httpClientFactory;
            _clientName = clientName;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                // Creamos un cliente fresco para este repositorio
                var client = _httpClientFactory.CreateClient(_clientName);

                var repositoryType = typeof(ApiRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), client);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            // En contexto API Stateless:
            // Las operaciones (POST/PUT/DELETE) se ejecutaron inmediatamente al llamar a Add/Update.
            // Retornamos 1 para indicar "Todo OK" al consumidor que espera confirmación.
            return await Task.FromResult(1);
        }

        public int SaveChanges()
        {
            return 1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // HttpClientFactory gestiona la vida de los clientes, no necesitamos hacer dispose manual usualmente.
                    _repositories?.Clear();
                }
            }
            _disposed = true;
        }
    }
}