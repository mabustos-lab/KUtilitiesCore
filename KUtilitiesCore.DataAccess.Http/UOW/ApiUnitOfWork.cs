using System;
using System.Collections;
using System.Collections.Generic; // Agregado para Dictionary
using System.Net.Http;
using System.Threading.Tasks;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using KUtilitiesCore.DataAccess.Http.Repositories;
using KUtilitiesCore.Dal.UOW;

namespace KUtilitiesCore.DataAccess.Http.UOW
{
    public class ApiUnitOfWork : IUnitOfWork
    {
        #region Fields

        private readonly string _clientName;

        // Diccionario para registrar repositorios personalizados
        private readonly Dictionary<Type, Type> _customRepositories = new Dictionary<Type, Type>();

        private readonly IHttpClientFactory _httpClientFactory;
        private bool _disposed;
        private Hashtable _repositories;

        #endregion Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpClientFactory">Fábrica de clientes HTTP de .NET Core.</param>
        /// <param name="clientName">
        /// Nombre del cliente configurado en Startup (con BaseAddress, Headers, etc).
        /// </param>
        public ApiUnitOfWork(IHttpClientFactory httpClientFactory, string clientName = "DefaultApi")
        {
            _httpClientFactory = httpClientFactory;
            _clientName = clientName;
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public TRepo RawRepository<TRepo>() where TRepo : IRawRepository
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(TRepo);
            if (!_repositories.ContainsKey(type))
            {
                // Creamos un cliente fresco para este repositorio
                var client = _httpClientFactory.CreateClient(_clientName);

                object repositoryInstance;

                // 1. Intentamos ver si hay un repositorio específico registrado
                if (_customRepositories.ContainsKey(typeof(TRepo)))
                {
                    var customRepoType = _customRepositories[typeof(TRepo)];
                    // Instanciamos usando el constructor validado previamente
                    repositoryInstance = Activator.CreateInstance(customRepoType, client);
                }
                else
                {
                    throw new UnregisteredRepositoryException($"Repositorio del tipo: {type.Name}, no esta registrado.");
                }

                _repositories.Add(type, repositoryInstance);
            }

            return (TRepo)_repositories[type];
        }

        /// <summary>
        /// Registra una implementación personalizada de un repositorio de lectura. Útil para
        /// registrar repositorios específicos de reportes que heredan de IRawRepository.
        /// </summary>
        /// <typeparam name="TInterface">La interfaz del repositorio (debe heredar de IRawRepository).</typeparam>
        public void RegisterCustomRepository<TInterface>() where TInterface : IRawRepository
        {
            var repoType = typeof(TInterface);

            // Validación Temprana: Verificamos si existe un constructor que acepte ApiUnitOfWork
            var constructor = repoType.GetConstructor(new[] { typeof(HttpClient) });

            if (constructor == null)
            {
                throw new ArgumentException(
                    $"El repositorio '{repoType.Name}' no tiene un constructor público que acepte un parámetro de tipo '{nameof(HttpClient)}'. " +
                    $"Esto es necesario para funcionar con ApiUnitOfWork.",
                   repoType.Name);
            }
            _customRepositories[repoType] = repoType;
        }

        /// <summary>
        /// Permite registrar un repositorio específico para una entidad. Útil cuando se requiere
        /// lógica personalizada para una entidad (ej. endpoints no estándar).
        /// </summary>
        public void RegisterCustomRepository<TEntity, TRepository>()
            where TEntity : class
            where TRepository : IRepository<TEntity>
        {
            var repoType = typeof(TRepository);

            // Validación Temprana: Verificamos si existe un constructor que acepte ApiUnitOfWork
            var constructor = repoType.GetConstructor(new[] { typeof(HttpClient) });

            if (constructor == null)
            {
                throw new ArgumentException(
                    $"El repositorio '{repoType.Name}' no tiene un constructor público que acepte un parámetro de tipo '{nameof(HttpClient)}'. " +
                    $"Esto es necesario para funcionar con ApiUnitOfWork.",
                   repoType.Name);
            }

            _customRepositories[repoType] = repoType;
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

                object repositoryInstance;

                // 1. Verificamos si hay un repositorio personalizado registrado
                if (_customRepositories.ContainsKey(typeof(T)))
                {
                    var customRepoType = _customRepositories[typeof(T)];

                    // Asumimos que el repositorio personalizado tiene un constructor que acepta HttpClient.
                    // Ejemplo: public ProductRepository(HttpClient client) : base(client) { }
                    repositoryInstance = Activator.CreateInstance(customRepoType, client);
                }
                else
                {
                    // 2. Fallback al repositorio genérico
                    var repositoryType = typeof(ApiRepository<>);
                    repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), client);
                }

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        public int SaveChanges()
        {
            return 1;
        }

        public async Task<int> SaveChangesAsync()
        {
            // En contexto API Stateless: Las operaciones (POST/PUT/DELETE) se ejecutaron
            // inmediatamente al llamar a Add/Update. Retornamos 1 para indicar "Todo OK" al
            // consumidor que espera confirmación.
            return await Task.FromResult(1);
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // HttpClientFactory gestiona la vida de los clientes, no necesitamos hacer
                    // dispose manual usualmente.
                    _repositories?.Clear();
                }
            }
            _disposed = true;
        }

        #endregion Protected Methods
    }
}