using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Contrato que agrupa el contexto de acceso a datos y la transacción activa. Garantiza que los
    /// repositorios operen dentro del mismo ámbito transaccional.
    /// </summary>
    public interface IDaoUowContext
    {
        #region Properties

        /// <summary>
        /// El contexto de acceso a datos (ejecutor de comandos).
        /// </summary>
        IDaoContext Context { get; }

        /// <summary>
        /// La transacción activa actual. Puede ser null si no se ha iniciado una transacción explícita.
        /// </summary>
        ITransaction Transaction { get; }

        #endregion Properties

        #region Methods

        void Rollback();

        #endregion Methods
    }

    public class DaoUowContext : IDaoUowContext, IDisposable
    {
        #region Fields

        private readonly IDaoContext _context;
        private ITransaction _transaction;
        private bool disposedValue;
        private bool isTransactionCreated;

        #endregion Fields

        #region Constructors

        public DaoUowContext(IDaoContext context)
        {
            _context = context;
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public IDaoContext Context => _context;

        /// <inheritdoc/>
        public ITransaction Transaction
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

        #region Methods

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

       
        public void Rollback()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                DisposeTransaction();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _transaction.Dispose();
                    _context.Dispose();
                }

                _transaction = null;
                disposedValue = true;
            }
        }

        internal void DisposeTransaction()
        {
            if(_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
            isTransactionCreated = true;
        }

        #endregion Methods
    }
}