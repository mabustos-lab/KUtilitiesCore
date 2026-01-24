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

        public RawRepositorybase(IDaoUowContext context)
        {
            _uowContext = context;
        }

        /// <summary>
        /// Acceso directo al contexto de datos (atajo).
        /// </summary>
        protected IDaoContext Context => _uowContext.Context;

        /// <summary>
        /// Acceso directo a la transacción (atajo).
        /// </summary>
        protected ITransaction Transaction => _uowContext.Transaction;
    }
}
