using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Implementacio para un repositorio generico para consultas que no mapean a una entidad
    /// </summary>
    public class RawRepositorybase : IRawRepository
    {
        private readonly IDaoUowContext _uowContext;

        /// <summary>
        /// Inicializa el repositorio con el contexto de unidad de trabajo del que obtiene
        /// la conexión y la transacción activa.
        /// </summary>
        /// <param name="context">Contexto de unidad de trabajo que agrupa la conexión y la transacción.</param>
        public RawRepositorybase(IDaoUowContext context)
        {
            _uowContext = context;
        }

        /// <summary>
        /// Acceso directo al contexto de datos (atajo).
        /// </summary>
        protected IDaoContext Context => _uowContext.Context;

        /// <summary>
        /// Acceso directo a la transacción (atajo). Puede ser null si no hay una transacción activa.
        /// </summary>
        protected ITransaction? Transaction => _uowContext.Transaction;
        /// <summary>
        /// Accesos directo a las funciones para obtener los repositorios
        /// </summary>
        protected IRepositoryProvider RepositoryProvider => _uowContext.DaoRepositoryProvider;
    }
}
