using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
#endif

namespace KUtilitiesCore.DataAccess.DAL
{
    class TransactionBase : ITransaction
    {
        #region Fields

        private readonly Guid _idTransaction;
        private bool _IsCommited;
        private bool _disposedValue;
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

#if NETCOREAPP
        /// <summary>
        /// Constructor para EF Core (Microsoft.EntityFrameworkCore)
        /// </summary>
        public TransactionBase(IDbContextTransaction transaction)
            : this(transaction.GetDbTransaction()) // Convertir a DbTransaction
        {
        }
#elif NETFRAMEWORK
        /// <summary>
        /// Constructor para EF6 (System.Data.Entity)
        /// </summary>
        public TransactionBase(System.Data.Entity.DbContextTransaction transaction)
            : this(transaction.UnderlyingTransaction) // Convertir a DbTransaction
        {
        }
#endif

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

        public DbTransaction GetTransactionObject()
        {
            return _transaction;
        }

        /// <summary>
        /// Libera los recursos asociados a la transacción.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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
