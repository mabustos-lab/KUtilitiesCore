using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.SQLLog
{
    public enum SqlLogLevel
    {
        Info,       // PRINT, mensajes informativos
        Warning,    // RAISERROR con severity <= 10
        Error,      // RAISERROR con severity > 10
        Debug       // Mensajes de depuración
    }
}
