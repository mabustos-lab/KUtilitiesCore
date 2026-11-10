using KUtilitiesCore.MVVM.MessageService;

namespace KUtilitiesCore.Data.WPFTests.TestDoubles
{
    /// <summary>
    /// Doble de prueba de <see cref="ISupportMessageService"/> que registra las
    /// notificaciones recibidas en lugar de mostrar diálogos, permitiendo asertar
    /// el enrutamiento de mensajes del ViewModel sin interacción de usuario.
    /// </summary>
    internal sealed class FakeMessageService : ISupportMessageService
    {
        private int _confirmCalls;

        /// <summary>Último <see cref="MessageArgs"/> notificado por cualquier vía.</summary>
        public MessageArgs? LastMessage { get; private set; }

        /// <summary>Número de invocaciones a <see cref="ShowAsync(MessageArgs)"/>.</summary>
        public int ShowAsyncCalls { get; private set; }

        /// <summary>Número de invocaciones a <see cref="ConfirmAsync(MessageArgs)"/>.</summary>
        public int ConfirmCalls => _confirmCalls;

        /// <summary>Resultado que devolverá <see cref="ConfirmAsync(MessageArgs)"/>.</summary>
        public bool ConfirmResult { get; set; } = true;

        /// <inheritdoc />
        public ValueTask ShowInfoAsync(string message)
            => ShowAsync(new MessageArgs { Message = message, MessageType = MessageType.Information });

        /// <inheritdoc />
        public ValueTask ShowWarningAsync(string message)
            => ShowAsync(new MessageArgs { Message = message, MessageType = MessageType.Warning });

        /// <inheritdoc />
        public ValueTask ShowSuccessAsync(string message)
            => ShowAsync(new MessageArgs { Message = message, MessageType = MessageType.Information });

        /// <inheritdoc />
        public ValueTask ShowErrorAsync(string message)
            => ShowAsync(new MessageArgs { Message = message, MessageType = MessageType.Error });

        /// <inheritdoc />
        public ValueTask ShowAsync(MessageArgs args)
        {
            ShowAsyncCalls++;
            LastMessage = args;
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask<bool> ConfirmAsync(MessageArgs args)
        {
            _confirmCalls++;
            LastMessage = args;
            return ValueTask.FromResult(ConfirmResult);
        }
    }
}
