using KUtilitiesCore.MVVM.MessageService;

namespace KUtilitiesCore.Data.WPF.MessageService
{
    /// <summary>
    /// Implementación WPF de <see cref="ISupportMessageService"/> que presenta los
    /// mensajes neutrales mediante <see cref="MessageDialogWindow"/>, el diálogo propio
    /// con anatomía TaskDialog (encabezado, detalles técnicos desplegables, pie de página
    /// e icono).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contrato de asincronía: todos los métodos devuelven <see cref="System.Threading.Tasks.ValueTask"/>
    /// completados de forma <b>síncrona</b> una vez que el diálogo modal se cierra
    /// (<c>Window.ShowDialog</c> bloquea el bucle de mensajes en el hilo de interfaz
    /// llamador). Esto preserva la firma asincrónica de la interfaz para los consumidores
    /// sin introducir saltos de contexto: quien necesite no bloquear el hilo debe
    /// invocarlo desde un contexto que lo tolere (el propio hilo de UI de WPF, que
    /// sigue bombeando mensajes durante <c>ShowDialog</c>).
    /// </para>
    /// <para>
    /// Se elige un diálogo XAML propio en lugar de <c>MessageBox</c> para soportar el
    /// expander de detalles técnicos (pila de la excepción) y el pie de página, secciones
    /// que el <c>MessageBox</c> nativo no ofrece.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var service = new WpfMessageService();
    /// await service.ShowAsync(new MessageArgs
    /// {
    ///     Caption = "Error de lectura",
    ///     Heading = "Error al leer el archivo.",
    ///     Message = ex.Message,
    ///     Expander = ex.ToString(),
    ///     MessageType = MessageType.Error,
    ///     MessageIcon = MessageIcon.Error
    /// });
    /// </code>
    /// </example>
    public sealed class WpfMessageService : ISupportMessageService
    {
        /// <summary>Muestra un mensaje informativo con icono de información.</summary>
        /// <param name="message">Texto del mensaje.</param>
        public ValueTask ShowInfoAsync(string message)
            => ShowAsync(new MessageArgs
            {
                Message = message,
                MessageType = MessageType.Information,
                MessageIcon = MessageIcon.Information
            });

        /// <summary>Muestra un mensaje de advertencia con icono de advertencia.</summary>
        /// <param name="message">Texto del mensaje.</param>
        public ValueTask ShowWarningAsync(string message)
            => ShowAsync(new MessageArgs
            {
                Message = message,
                MessageType = MessageType.Warning,
                MessageIcon = MessageIcon.Warning
            });

        /// <summary>Muestra un mensaje de éxito con icono de información.</summary>
        /// <param name="message">Texto del mensaje.</param>
        public ValueTask ShowSuccessAsync(string message)
            => ShowAsync(new MessageArgs
            {
                Message = message,
                MessageType = MessageType.Information,
                MessageIcon = MessageIcon.Information
            });

        /// <summary>Muestra un mensaje de error con icono de error.</summary>
        /// <param name="message">Texto del mensaje.</param>
        public ValueTask ShowErrorAsync(string message)
            => ShowAsync(new MessageArgs
            {
                Message = message,
                MessageType = MessageType.Error,
                MessageIcon = MessageIcon.Error
            });

        /// <summary>Muestra el mensaje completo descrito por <paramref name="args"/>.</summary>
        /// <param name="args">Argumentos neutrales del mensaje a presentar.</param>
        public ValueTask ShowAsync(MessageArgs args)
        {
            ShowCore(args, isConfirmation: false);
            return ValueTask.CompletedTask;
        }

        /// <summary>Muestra el mensaje descrito por <paramref name="args"/> pidiendo confirmación al usuario.</summary>
        /// <param name="args">Argumentos neutrales del mensaje a presentar.</param>
        /// <returns><see langword="true"/> si el usuario confirmó ("Sí"); <see langword="false"/> en caso contrario.</returns>
        public ValueTask<bool> ConfirmAsync(MessageArgs args)
            => ValueTask.FromResult(ShowCore(args, isConfirmation: true));

        private static bool ShowCore(MessageArgs args, bool isConfirmation)
        {
            ArgumentNullException.ThrowIfNull(args);
            var window = new MessageDialogWindow(MessageDialogModel.FromArgs(args, isConfirmation));
            window.ShowDialog();
            return window.DialogResult == true;
        }
    }
}
