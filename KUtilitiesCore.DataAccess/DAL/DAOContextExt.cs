using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.DAL
{
    public static class DAOContextExt
    {
        /// <summary>
        /// Obtiene el nombre del servidor publicado asociado al contexto de datos.
        /// </summary>
        /// <returns>El nombre del servidor publicado como una cadena.</returns>
        public static string GetPublishedServerName<TDAO>(this TDAO context) where TDAO : IDAOContext
        { return context.Scalar<string>("SELECT PUBLISHINGSERVERNAME() as Servername"); }

        /// <summary>
        /// Obtiene la fecha y hora actual del servidor.
        /// </summary>
        /// <returns>Un <see cref="DateTime"/> que representa la fecha y hora actual del servidor.</returns>
        public static DateTime GetServerDateTime<TDAO>(this TDAO context) where TDAO : IDAOContext
        { return context.Scalar<DateTime>("SELECT SYSDATETIME() AS FechaHoraActual"); }
    }
}
