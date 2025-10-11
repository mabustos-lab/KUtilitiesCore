using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.SPHelper
{
    /// <summary>
    /// Clase para fabricar instancia de IStoreProcedureHelper
    /// </summary>
    public static class SpHelperFactory
    {
        /// <summary>
        /// Crea una instancia de IStoreProcedureHelper
        /// </summary>
        /// <param name="context">Contexto de conección a datos</param>
        /// <param name="storeProcedureName">Nombre del procedimiento almacenado a ejecutar</param>
        /// <returns></returns>
        public static IStoreProcedureHelper Create(IDaoContext context, string storeProcedureName)
        {
            return new StoreProcedureHelper(context, storeProcedureName);
        }
    }
}
