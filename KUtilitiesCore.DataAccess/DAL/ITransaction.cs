using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.DAL
{
    /// <summary>
    /// Define una interfaz para manejar transacciones, proporcionando métodos para confirmar o revertir cambios,
    /// así como para liberar recursos asociados a la transacción.
    /// </summary>
    /// <remarks>Si no se ha llamado al Commit antes de hacer Dispose, automáticamente se hará un RollBack</remarks>
    public interface ITransaction : IDisposable
    {
        #region Methods

        /// <summary>
        /// Confirma todos los cambios realizados durante la transacción actual, marcando su finalización exitosa.
        /// </summary>
        /// <remarks>
        /// Este método debe ser llamado únicamente cuando todos los pasos de la transacción se hayan completado correctamente.
        /// </remarks>
        void Commit();

        /// <summary>
        /// Revierte todos los cambios realizados durante la transacción actual, devolviendo el estado al momento
        /// en que la transacción fue iniciada o a un punto de retorno definido dentro de la misma.
        /// </summary>
        /// <remarks>
        /// Este método debe ser utilizado para manejar errores o inconsistencias detectadas durante la transacción.
        /// </remarks>
        void Rollback();

        #endregion Methods
    }

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
        /// <param name="transacFactory">Función que crea una nueva transacción.</param>
        public TransactionBase(DbTransaction transaction)
        {
            this._idTransaction = Guid.NewGuid();
            this._transaction = transaction;
            this._IsCommited = false;
            this._disposedValue = false;
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
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos asociados.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> si se está liberando recursos administrados; en caso contrario, <c>false</c>.
        /// </param>
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
