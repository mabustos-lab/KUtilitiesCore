using System.Data.Common;
using System.Diagnostics;

namespace KUtilitiesCore.DataAccess.DAL
{
    public class TransactionBase : ITransaction
    {
        #region Fields

        private readonly Guid _idTransaction;
        private bool _disposedValue;
        private bool _IsCommited;
        private DbTransaction _transaction;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase con los delegados para commit y rollback.
        /// </summary>
        public TransactionBase(DbTransaction transaction)
        {
            _idTransaction = Guid.NewGuid();
            _transaction = transaction;
            _IsCommited = false;
            _disposedValue = false;
            Debug.WriteLine($"Se inicia una Transacción ID: {_idTransaction}", "Transacción");
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Confirma los cambios realizados en la transacción.
        /// </summary>
        public virtual void Commit()
        {
            if (_IsCommited || _transaction == null)
            {
                return;
            }

            _IsCommited = true;
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        /// <summary>
        /// Libera los recursos asociados a la transacción.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public DbTransaction GetTransactionObject()
        {
            return _transaction;
        }

        /// <summary>
        /// Revierte los cambios realizados en la transacción.
        /// </summary>
        public virtual void Rollback()
        {
            if (_IsCommited || _transaction == null)
            {
                return;
            }

            try
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
                Debug.WriteLine($"Se realizó un Rollback en la Transacción ID: {_idTransaction}", "Transacción");
            }
            catch (Exception ex)
            {
                Debug.Fail($"Error al realizar Rollback en la Transacción ID: {_idTransaction}", ex.ToString());
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing && !_IsCommited && _transaction != null)
            {
                Rollback();
            }

            _disposedValue = true;
        }

        #endregion Methods
    }
}