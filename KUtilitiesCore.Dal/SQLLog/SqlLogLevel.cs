using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.SQLLog
{
    /// <summary>
    /// Niveles de severidad de los mensajes SQL capturados por el logging de conexiones,
    /// clasificados según el mecanismo de origen del mensaje.
    /// </summary>
    public enum SqlLogLevel
    {
        /// <summary>Mensajes informativos generados por PRINT.</summary>
        Info,
        /// <summary>Advertencias generadas por RAISERROR con severidad menor o igual a 10.</summary>
        Warning,
        /// <summary>Errores generados por RAISERROR con severidad mayor a 10.</summary>
        Error,
        /// <summary>Mensajes de depuración de bajo nivel.</summary>
        Debug
    }
}
