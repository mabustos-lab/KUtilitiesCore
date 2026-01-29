using System;
using System.Collections;
using System.Threading.Tasks;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using KUtilitiesCore.Dal;
using KUtilitiesCore.Extensions;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Implementación de Unit of Work para KUtilitiesCore.Dal. Gestiona transacciones SQL nativas a
    /// través de IDAOContext.
    /// </summary>
    public class DaoUnitOfWork : IUnitOfWork, IDaoRepositoryProvider
    {
        /// <summary>
        /// Contexto transaccional proporcionado por el Unit of Work.
        /// </summary>
        protected readonly IDaoUowContext UowContext;

        // Registro opcional de tipos personalizados: <TipoEntidad, TipoRepositorio>
        private readonly Dictionary<Type, Type> _customRepositories = [];

        private bool _disposed;
        private Hashtable _repositories;

        public DaoUnitOfWork(IDaoContext context)
        {
            UowContext = new DaoUowContext(this, context);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Registra una implementación personalizada de un repositorio de lectura.
        /// Útil para registrar repositorios específicos de reportes que heredan de IRawRepository.
        /// </summary>
        /// <typeparam name="TInterface">La interfaz del repositorio (debe heredar de IRawRepository).</typeparam>
        public void RegisterCustomRepository<TInterface>() where TInterface : IRawRepository
        {
            var repoType = typeof(TInterface);

            // Validación Temprana: Verificamos si existe un constructor que acepte IDaoUowContext
            var constructor = repoType.GetConstructor(new[] { typeof(IDaoUowContext) });

            if (constructor == null)
            {
                throw new ArgumentException(
                    $"El repositorio '{repoType.Name}' no tiene un constructor público que acepte un parámetro de tipo '{nameof(IDaoUowContext)}'. " +
                    $"Esto es necesario para funcionar con DaoUnitOfWork.",
                   repoType.Name);
            }
            _customRepositories[repoType] = repoType;
            
        }

        /// <summary>
        /// Permite registrar un repositorio específico para una entidad.
        /// Realiza validación temprana del constructor.
        /// </summary>
        public void RegisterCustomRepository<TEntity, TRepository>()
            where TEntity : class
            where TRepository : IRepository<TEntity>
        {
            var repoType = typeof(TRepository);

            // Validación Temprana: Verificamos si existe un constructor que acepte IDaoUowContext
            var constructor = repoType.GetConstructor(new[] { typeof(IDaoUowContext) });

            if (constructor == null)
            {
                throw new ArgumentException(
                    $"El repositorio '{repoType.Name}' no tiene un constructor público que acepte un parámetro de tipo '{nameof(IDaoUowContext)}'. " +
                    $"Esto es necesario para funcionar con DaoUnitOfWork.",
                   repoType.Name);
            }

            _customRepositories[typeof(TEntity)] = repoType;
        }

        /// <inheritdoc/>
        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                object repositoryInstance;

                // 1. Intentamos ver si hay un repositorio específico registrado
                if (_customRepositories.ContainsKey(typeof(T)))
                {
                    var customRepoType = _customRepositories[typeof(T)];
                    // Instanciamos usando el constructor validado previamente
                    repositoryInstance = Activator.CreateInstance(customRepoType, UowContext);
                }
                else
                {
                    // 2. Fallback al genérico (DefaultDaoRepository)
                    var repositoryType = typeof(DefaultDaoRepository<>);
                    repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), UowContext);
                }

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        /// <inheritdoc/>
        public TRepo RawRepository<TRepo>() where TRepo : IRawRepository
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(TRepo);
            if (!_repositories.ContainsKey(type))
            {
                object repositoryInstance;

                // 1. Intentamos ver si hay un repositorio específico registrado
                if (_customRepositories.ContainsKey(typeof(TRepo)))
                {
                    var customRepoType = _customRepositories[typeof(TRepo)];
                    // Instanciamos usando el constructor validado previamente
                    repositoryInstance = Activator.CreateInstance(customRepoType, UowContext);
                }
                else
                {
                    throw new UnregisteredRepositoryException($"Repositorio del tipo: {type.Name}, no esta registrado.");
                    //// 2. Fallback al genérico (DefaultDaoRepository)
                    //var repositoryType = typeof(RawRepositorybase);
                    //repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TRepo)), UowContext);
                }

                _repositories.Add(type, repositoryInstance);
            }

            return (TRepo)_repositories[type];
        }

        /// <inheritdoc/>
        public int SaveChanges()
        {
            try
            {
                if (UowContext.Transaction != null)
                {
                    UowContext.Transaction.Commit();

                    // Importante: No ponemos a null inmediatamente la transacción aquí si queremos permitir 
                    // múltiples SaveChanges en el mismo scope, pero en patrón UOW clásico, 
                    // SaveChanges suele marcar el fin.
                    // Reiniciamos para evitar re-commit de lo mismo.
                    DisposeTransaction();
                    return 1;
                }
                return 0;
            }
            catch
            {
                Rollback();
                throw;
            }
        }
        /// <summary>
        /// Revierte todos los cambios realizados en la UOW actual.
        /// </summary>
        public void Rollback()
        {
            if (UowContext.Transaction != null)
            {
                UowContext.Transaction.Rollback();
                DisposeTransaction();
            }
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

        private void DisposeTransaction()
        {
            if (UowContext.Transaction != null)
            {
                ((DaoUowContext)UowContext).DisposeTransaction();
                // Limpiamos repositorios para que futuras llamadas obtengan un contexto limpio
                _repositories?.Clear();
            }
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    
                    ((DaoUowContext)UowContext).Dispose();
                }
            }
            _disposed = true;
        }

    }
}