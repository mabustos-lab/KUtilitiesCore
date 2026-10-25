using System;
using System.Collections;
using System.Data;
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
    public class DaoUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Contexto transaccional proporcionado por el Unit of Work.
        /// </summary>
        protected readonly IDaoUowContext UowContext;

        // Registro opcional de tipos personalizados: <TipoEntidad, TipoRepositorio>
        private readonly Dictionary<Type, Type> _customRepositories = [];

        private bool _disposed;
        private readonly Hashtable _repositories = new();

        /// <summary>
        /// Inicializa la unidad de trabajo creando el contexto transaccional que envolverá
        /// a todos los repositorios que resuelva esta instancia.
        /// </summary>
        /// <param name="context">Contexto de acceso a datos sobre el que se administrarán las transacciones.</param>
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
            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                object repositoryInstance;

                // 1. Intentamos ver si hay un repositorio específico registrado
                if (_customRepositories.ContainsKey(typeof(T)))
                {
                    var customRepoType = _customRepositories[typeof(T)];
                    // Instanciamos usando el constructor validado previamente
                    repositoryInstance = Activator.CreateInstance(customRepoType, UowContext)
                        ?? throw new InvalidOperationException(
                            $"No se pudo crear la instancia del repositorio '{customRepoType.Name}'.");
                }
                else
                {
                    // 2. Fallback al genérico (DefaultDaoRepository)
                    var repositoryType = typeof(DefaultDaoRepository<>);
                    repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), UowContext)
                        ?? throw new InvalidOperationException(
                            $"No se pudo crear la instancia del repositorio para '{typeof(T).Name}'.");
                }

                _repositories.Add(type, repositoryInstance);
            }

            var storedRepository = _repositories[type]
                ?? throw new InvalidOperationException($"No se encontró el repositorio registrado para '{type}'.");
            return (IRepository<T>)storedRepository;
        }

        /// <inheritdoc/>
        public TRepo RawRepository<TRepo>() where TRepo : IRawRepository
        {
            var type = typeof(TRepo);
            if (!_repositories.ContainsKey(type))
            {
                object repositoryInstance;

                // 1. Intentamos ver si hay un repositorio específico registrado
                if (_customRepositories.ContainsKey(typeof(TRepo)))
                {
                    var customRepoType = _customRepositories[typeof(TRepo)];
                    // Instanciamos usando el constructor validado previamente
                    repositoryInstance = Activator.CreateInstance(customRepoType, UowContext)
                        ?? throw new InvalidOperationException(
                            $"No se pudo crear la instancia del repositorio '{customRepoType.Name}'.");
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

            var storedRepository = _repositories[type]
                ?? throw new InvalidOperationException($"No se encontró el repositorio registrado para '{type.Name}'.");
            return (TRepo)storedRepository;
        }

        /// <summary>
        /// Abre una transacción explícita sobre el contexto del UoW y la registra en su estado,
        /// de modo que los repositorios resueltos la compartan y que <see cref="SaveChanges"/>/
        /// <see cref="Rollback"/> actúen sobre ella.
        /// </summary>
        /// <param name="isolationLevel">Nivel de aislamiento de la transacción (por defecto ReadCommitted).</param>
        /// <returns>La transacción creada y registrada en el UoW.</returns>
        /// <example>
        /// Patrón de uso con confirmación explícita. Las operaciones aceptan la transacción
        /// por parámetro opcional; si no se llama a <see cref="ITransaction.Commit"/>,
        /// la liberación de la transacción revierte automáticamente los cambios:
        /// <code>
        /// var context = new DaoContext(connectionBuilder);
        /// using var unitOfWork = new DaoUnitOfWork(context);
        /// using (var transaction = unitOfWork.BeginTransaction())
        /// {
        ///     context.ExecuteNonQuery(sql, parameters, transaction: transaction);
        ///     transaction.Commit(); // sin esta llamada, Dispose revierte los cambios
        /// }
        /// </code>
        /// </example>
        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var transaction = UowContext.Context.BeginTransaction(isolationLevel);
            ((DaoUowContext)UowContext).RegisterTransaction(transaction);
            return transaction;
        }

        /// <summary>
        /// Confirma la transacción registrada en el UoW cuando existe y está viva.
        /// </summary>
        /// <returns>
        /// 1 si confirmó la transacción registrada; 0 cuando no hay ninguna transacción
        /// registrada (las operaciones ejecutadas sin enlistar operan en autocommit y no
        /// requieren confirmación).
        /// </returns>
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
        /// Revierte la transacción registrada en el UoW, si existe y está viva, y deja el UoW
        /// sin transacción registrada. Sin transacción registrada es un no-op silencioso.
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
        /// Confirma de forma asíncrona la transacción registrada en el UoW cuando existe y
        /// está viva, delegando en <see cref="SaveChanges"/>.
        /// </summary>
        /// <returns>
        /// 1 si confirmó la transacción registrada; 0 cuando no hay ninguna transacción
        /// registrada (las operaciones ejecutadas sin enlistar operan en autocommit y no
        /// requieren confirmación).
        /// </returns>
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