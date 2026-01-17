using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Contexto de conexión usado por los repositorios del tipo DAO
    /// </summary>
    public interface IDaoUowContext
    {
        /// <summary>
        /// Interfaz desacoplada de la base de datos para el acceso a datos.
        /// </summary>
        IDaoContext Context { get; }
        /// <summary>
        /// Interfaz desacoplada de la base de datos para las transacciones de datos unidicada por el UOW.
        /// </summary>
        ITransaction Transaction { get; }
    }

    public class DaoUowContext : IDaoUowContext
    {
        private readonly Func<IDaoContext> _contexDelegate;
        private readonly Func<ITransaction> _transactionDelegate;

        public DaoUowContext(Func<IDaoContext> contexDelegate, Func<ITransaction> transactionDelegate)
        {
            _contexDelegate = contexDelegate;
            _transactionDelegate = transactionDelegate;
        }
        /// <inheritdoc/>
        public IDaoContext Context => _contexDelegate?.Invoke();

        /// <inheritdoc/>
        public ITransaction Transaction => _transactionDelegate?.Invoke();
    }
}
