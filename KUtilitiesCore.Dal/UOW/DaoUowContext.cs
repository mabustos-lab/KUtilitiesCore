using System;
using System.Linq;

namespace KUtilitiesCore.Dal.UOW
{
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
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
            isTransactionCreated = true;
        }

        #endregion Methods
    }
}