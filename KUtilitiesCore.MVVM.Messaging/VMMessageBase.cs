using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Clase base para mensajes que pueden definir una condición para ser procesados.
    /// </summary>
    public abstract class VMMessageBase : IConditionalMessage // Implementa IConditionalMessage
    {
        // Este predicado parece estar pensado para ser evaluado por el emisor o el receptor,
        // no directamente por el Messenger, a menos que se modifique el Messenger para ello.
        internal Func<object, bool> ShouldProcessPredicate { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="VMMessageBase"/>.
        /// </summary>
        /// <param name="shouldProcessPredicate">Un predicado que determina si el mensaje debe ser procesado por un destinatario específico.</param>
        protected VMMessageBase(Func<object, bool> shouldProcessPredicate)
        {
            ShouldProcessPredicate = shouldProcessPredicate;
        }

        /// <summary>
        /// Determina si el mensaje debe ser procesado por el destinatario/módulo proporcionado.
        /// </summary>
        /// <param name="recipientContext">El contexto del destinatario (por ejemplo, la instancia del destinatario) para evaluar la condición.</param>
        /// <returns><c>true</c> si el mensaje debe ser procesado; de lo contrario, <c>false</c>.</returns>
        public abstract bool ShouldProcess(object recipientContext);
    }
}
