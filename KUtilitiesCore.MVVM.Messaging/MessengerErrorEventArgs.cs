using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Argumentos para el evento de error del Messenger.
    /// </summary>
    public class MessengerErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Obtiene el destinatario que estaba procesando el mensaje cuando ocurrió el error. Puede ser null si el destinatario fue recolectado.
        /// </summary>
        public object? Recipient { get; }

        /// <summary>
        /// Obtiene el mensaje que se estaba procesando.
        /// </summary>
        public object? Message { get; }

        /// <summary>
        /// Obtiene la excepción que ocurrió.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="MessengerErrorEventArgs"/>.
        /// </summary>
        /// <param name="recipient">El destinatario.</param>
        /// <param name="message">El mensaje.</param>
        /// <param name="error">La excepción.</param>
        public MessengerErrorEventArgs(object? recipient, object? message, Exception error)
        {
            Recipient = recipient;
            Message = message;
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
