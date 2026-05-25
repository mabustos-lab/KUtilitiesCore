using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.MessageService
{
    /// <summary>
    /// Define los iconos disponibles para mostrar en los servicios de mensajes.
    /// </summary>
    public enum MessageIcon
    {
        /// <summary>Sin icono.</summary>
        None,
        /// <summary>Icono de información.</summary>
        Information,
        /// <summary>Icono de advertencia.</summary>
        Warning,
        /// <summary>Icono de error.</summary>
        Error,
        /// <summary>Icono de pregunta.</summary>
        Question
    }
}
