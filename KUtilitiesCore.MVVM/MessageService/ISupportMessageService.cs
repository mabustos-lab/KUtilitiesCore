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
        /// <summary>
        /// Muestra un mensaje con un tipo e icono específicos de forma asíncrona.
        /// </summary>
        /// <param name="message">El contenido del mensaje.</param>
        /// <param name="caption">El título de la ventana del mensaje.</param>
        /// <param name="type">El tipo de mensaje (<see cref="MessageType"/>).</param>
        /// <param name="icon">El icono a mostrar (<see cref="MessageIcon"/>).</param>
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowAsync(string message, string caption, MessageType type, MessageIcon icon = MessageIcon.None);

        /// <summary>
        /// Muestra un mensaje de confirmación al usuario y devuelve el resultado de forma asíncrona.
        /// </summary>
        /// <param name="message">La pregunta o mensaje de confirmación.</param>
        /// <param name="caption">El título de la ventana del mensaje.</param>
        /// <param name="icon">El icono de pregunta a mostrar (por defecto <see cref="MessageIcon.Question"/>).</param>
        /// <returns>Una <see cref="ValueTask{Boolean}"/> que devuelve true si el usuario confirma, de lo contrario false.</returns>
        ValueTask<bool> ConfirmAsync(string message, string caption, MessageIcon icon = MessageIcon.Question);
    }

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
