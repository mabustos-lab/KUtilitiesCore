using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Implementación estándar de <see cref="VMMessageBase"/>.
    /// </summary>
    public class VMMessage : VMMessageBase
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="VMMessage"/>.
        /// </summary>
        /// <param name="shouldProcessPredicate">Un predicado que determina si el mensaje debe ser procesado.</param>
        public VMMessage(Func<object, bool> shouldProcessPredicate) : base(shouldProcessPredicate)
        {
        }

        /// <inheritdoc/>
        public override bool ShouldProcess(object recipientContext)
        {
            return ShouldProcessPredicate?.Invoke(recipientContext) ?? true;
        }
    }
}
