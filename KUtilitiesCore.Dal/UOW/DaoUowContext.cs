using KUtilitiesCore.Dal.Helpers;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Proporciona un contexto de unidad de trabajo para operaciones de acceso a datos,
    /// manteniendo el contexto de datos, el proveedor de repositorios y la transacción
    /// registrada dentro de un único ámbito.
    /// </summary>
    /// <remarks>
    /// DaoUowContext coordina la vida útil de un contexto de acceso a datos y sus
    /// repositorios asociados, lo que permite compartir una transacción entre múltiples
    /// repositorios. Implementa IDisposable para liberar los recursos al completar la
    /// unidad de trabajo. Este contexto nunca abre transacciones implícitas: la
    /// transacción solo existe si el consumidor la crea y la registra explícitamente
    /// (p. ej. mediante <see cref="DaoUnitOfWork.BeginTransaction"/>), de modo que la
    /// coherencia y la atomicidad de un grupo de operaciones son responsabilidad del
    /// consumidor, que debe enlistar cada operación en dicha transacción.
    /// </remarks>
    public class DaoUowContext : IDaoUowContext, IDisposable
    {
        #region Fields

        private readonly IDaoContext _context;
        private readonly IRepositoryProvider _DaoRepositoryProvider;
        private ITransaction? _transaction;
        private bool disposedValue;

        #endregion Fields

        #region Public Constructors

        /// <summary>
        /// Inicializa el contexto de unidad de trabajo con el proveedor de repositorios y el
        /// contexto de acceso a datos que compartirán todas las operaciones transaccionales.
        /// </summary>
        /// <param name="provider">Proveedor que resuelve las instancias de los repositorios.</param>
        /// <param name="context">Contexto de acceso a datos sobre el que se administran las transacciones.</param>
        public DaoUowContext(IRepositoryProvider provider, IDaoContext context)

        {
            _context = context;
            _DaoRepositoryProvider = provider;
        }

        #endregion Public Constructors

        #region Properties

        /// <inheritdoc/>
        public IDaoContext Context => _context;

        /// <inheritdoc/>
        public IRepositoryProvider DaoRepositoryProvider => _DaoRepositoryProvider;

        /// <inheritdoc/>
        public ITransaction? Transaction
        {
            get
            {
                // Verificación de vitalidad (design D4): si el consumidor finalizó la
                // transacción registrada fuera del UoW (Commit/Rollback/Dispose), la
                // referencia interna queda obsoleta y se anula para no exponerla.
                if (_transaction is TransactionBase baseTransaction
                    && baseTransaction.GetTransactionObject() == null)
                {
                    _transaction = null;
                }
                return _transaction;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Libera la transacción activa y el contexto de acceso a datos asociado.
        /// </summary>
        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Revierte la transacción activa, si existe, y libera sus recursos.
        /// </summary>
        public void Rollback()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                DisposeTransaction();
            }
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Registra una transacción explícita como la transacción activa del UoW.
        /// El getter <see cref="Transaction"/> es pasivo: solo devuelve lo registrado aquí,
        /// jamás abre transacciones por su cuenta.
        /// </summary>
        /// <param name="transaction">La transacción a registrar.</param>
        internal void RegisterTransaction(ITransaction transaction)
        {
            _transaction = transaction;
        }

        /// <summary>
        /// Libera la transacción registrada y anula la referencia, de forma idempotente.
        /// </summary>
        internal void DisposeTransaction()
        {
            _transaction?.Dispose();
            _transaction = null;
        }

        #endregion Internal Methods

        #region Protected Methods

        /// <summary>
        /// Libera la transacción activa y el contexto subyacente cuando la liberación
        /// proviene de una llamada explícita a <see cref="Dispose()"/>.
        /// </summary>
        /// <param name="disposing">Indica si la llamada proviene de Dispose() y no del finalizador.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }

                _transaction = null;
                disposedValue = true;
            }
        }

        #endregion Protected Methods
    }
}