using KUtilitiesCore.Dal.Helpers;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Dal.UOW
{
    /// <resumen>
    /// Proporciona un contexto de unidad de trabajo para operaciones de acceso a datos, gestionando transacciones y acceso al repositorio dentro de un
    /// único ámbito.
    /// </resumen>
    /// <remarks>DaoUowContext coordina la vida útil de un contexto de acceso a datos y sus repositorios asociados,
    /// lo que permite operaciones transaccionales en múltiples repositorios. Implementa IDisposable para
    /// garantizar que los recursos se liberen cuando se complete la unidad de trabajo. Esta clase se utiliza normalmente para agrupar
    /// operaciones de datos relacionadas en una única transacción, lo que garantiza la coherencia y la atomicidad.</remarks>
    public class DaoUowContext : IDaoUowContext, IDisposable
    {
        #region Fields

        private readonly IDaoContext _context;
        private readonly IRepositoryProvider _DaoRepositoryProvider;
        private ITransaction? _transaction;
        private bool disposedValue;
        private bool isTransactionCreated;

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
                if (isTransactionCreated)
                {
                    _transaction = _context.BeginTransaction();
                    isTransactionCreated = true;
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

        internal void DisposeTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
            isTransactionCreated = true;
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