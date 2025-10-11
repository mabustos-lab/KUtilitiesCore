using System;
using System.Linq;


namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Define un mensaje que puede controlar condicionalmente si debe ser procesado por un destinatario.
    /// </summary>
    public interface IConditionalMessage
    {
        /// <summary>
        /// Determina si el mensaje debe ser procesado por el contexto del destinatario proporcionado.
        /// </summary>
        /// <param name="recipientContext">El contexto del destinatario (usualmente, la instancia del objeto destinatario).</param>
        /// <returns><c>true</c> si el mensaje debe ser procesado; de lo contrario, <c>false</c>.</returns>
        bool ShouldProcess(object recipientContext);
    }
}
