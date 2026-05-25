using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.MessageService
{
    /// <summary>
    /// Define las categorías o tipos de mensajes.
    /// </summary>
    public enum MessageType
    {
        /// <summary>Mensaje genérico sin categoría específica.</summary>
        Generic,
        /// <summary>Mensaje de carácter informativo.</summary>
        Information,
        /// <summary>Mensaje de advertencia sobre una posible situación anómala.</summary>
        Warning,
        /// <summary>Mensaje que indica un error o fallo.</summary>
        Error
    }
}
