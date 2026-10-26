using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Contrato que agrupa el contexto de acceso a datos y la transacción registrada en el UoW,
    /// permitiendo que los repositorios resueltos compartan ambos. La transacción existe solo
    /// si el consumidor la registra explícitamente; este contrato no promete transaccionalidad
    /// implícita ni atomicidad automática de las operaciones.
    /// </summary>
    public interface IDaoUowContext
    {
        #region Properties

        /// <summary>
        /// El contexto de acceso a datos (ejecutor de comandos).
        /// </summary>
        IDaoContext Context { get; }

        /// <summary>
        /// La transacción registrada en el UoW, o null si no existe ninguna viva.
        /// </summary>
        /// <remarks>
        /// Este miembro es pasivo: devuelve únicamente la transacción registrada explícitamente
        /// (p. ej. mediante <c>DaoUnitOfWork.BeginTransaction</c>) y jamás abre una por su cuenta.
        /// Si la transacción registrada fue finalizada fuera del UoW (Commit, Rollback o Dispose),
        /// devuelve null.
        /// </remarks>
        ITransaction? Transaction { get; }
        /// <summary>
        /// Expone el acceso a las instancias de los repositorion en el UOW
        /// </summary>
        IRepositoryProvider DaoRepositoryProvider { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Revierte la transacción registrada en el UoW, si existe y está viva, y libera sus
        /// recursos. Sin transacción registrada es un no-op silencioso.
        /// </summary>
        void Rollback();

        #endregion Methods
    }
}