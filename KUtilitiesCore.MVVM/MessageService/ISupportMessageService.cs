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
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowInfoAsync(string message);

        /// <summary>
        /// Muestra un mensaje de advertencia al usuario de forma asíncrona.
        /// </summary>
        /// <param name="message">El contenido del mensaje de advertencia.</param>
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowWarningAsync(string message);
        /// <summary>
        /// Muestra un mensaje de advertencia al usuario de forma asíncrona.
        /// </summary>
        /// <param name="message">El contenido del mensaje de advertencia.</param>
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowSuccessAsync(string message); 
        /// <summary>
        /// Muestra un mensaje de error al usuario de forma asíncrona.
        /// </summary>
        /// <param name="message">El contenido del mensaje de error.</param>
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowErrorAsync(string message);
        /// <summary>
        /// Muestra un mensaje con un tipo e icono específicos de forma asíncrona.
        /// </summary>
        /// <param name="messageArgs">Propiedades que contienen la información del mensaje que se mostrará.</param>
        /// <returns>Una <see cref="ValueTask"/> que representa la operación asíncrona.</returns>
        ValueTask ShowAsync(MessageArgs messageArgs);

        /// <summary>
        /// Muestra un mensaje de confirmación al usuario y devuelve el resultado de forma asíncrona.
        /// </summary>
        /// <param name="messageArgs">Propiedades que contienen la información del mensaje que se mostrará.</param>
        /// <returns>Una <see cref="ValueTask{Boolean}"/> que devuelve true si el usuario confirma, de lo contrario false.</returns>
        ValueTask<bool> ConfirmAsync(MessageArgs messageArgs);
    }
}
