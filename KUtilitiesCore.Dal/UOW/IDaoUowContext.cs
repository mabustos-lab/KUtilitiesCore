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
        /// <summary>
        /// Expone el acceso a las instancias de los repositorion en el UOW
        /// </summary>
        IDaoRepositoryProvider DaoRepositoryProvider { get; }

        #endregion Properties

        #region Methods

        void Rollback();

        #endregion Methods
    }
}