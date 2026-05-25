using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.MessageService
{
    /// <summary>
    /// Define la funcionalidad para mostrar mensajes en la interfaz de usuario de forma asíncrona e independiente de la tecnología de UI.
    /// </summary>
    public interface ISupportMessageService
    {
        /// <summary>
        /// Muestra un mensaje de información al usuario de forma asíncrona.
        /// </summary>
        /// <param name="message">El contenido del mensaje a mostrar.</param>
        /// <param name="caption">El título de la ventana del mensaje (por defecto "Información").</param>
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowInfoAsync(string message, string caption = "Información");

        /// <summary>
        /// Muestra un mensaje de advertencia al usuario de forma asíncrona.
        /// </summary>
        /// <param name="message">El contenido del mensaje de advertencia.</param>
        /// <param name="caption">El título de la ventana del mensaje (por defecto "Advertencia").</param>
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowWarningAsync(string message, string caption = "Advertencia");

        /// <summary>
        /// Muestra un mensaje de error al usuario de forma asíncrona.
        /// </summary>
        /// <param name="message">El contenido del mensaje de error.</param>
        /// <param name="caption">El título de la ventana del mensaje (por defecto "Error").</param>
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowErrorAsync(string message, string caption = "Error");
        ValueTask ShowAsync(string message, string caption, MessageType type, MessageIcon icon = MessageIcon.None);

        ValueTask<bool> ConfirmAsync(string message, string caption, MessageIcon icon = MessageIcon.Question);
    }
    public enum MessageIcon
    {
        None,
        Information,
        Warning,
        Error,
        Question
    }
    public enum MessageType
    {
        Generic,
        Information,
        Warning,
        Error
    }
}
