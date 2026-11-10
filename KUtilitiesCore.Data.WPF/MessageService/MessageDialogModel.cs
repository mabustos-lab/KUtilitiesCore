using KUtilitiesCore.MVVM.MessageService;

namespace KUtilitiesCore.Data.WPF.MessageService
{
    /// <summary>
    /// Modelo inmutable del diálogo de mensajes de WPF: proyección pura y testeable de
    /// <see cref="MessageArgs"/> hacia el estado que consume <see cref="MessageDialogWindow"/>.
    /// No contiene lógica de UI ni depende del hilo de interfaz, de modo que su mapeo
    /// se valida con pruebas unitarias convencionales.
    /// </summary>
    /// <remarks>
    /// <para>
    /// El modelo decide qué secciones del diálogo se muestran mediante los indicadores
    /// <see cref="ShowHeading"/>, <see cref="ShowFootnote"/> y <see cref="ShowExpander"/>:
    /// un mensaje sin pie de página o sin detalles técnicos simplemente omite esas secciones
    /// (anatomía TaskDialog), en lugar de renderizar espacios vacíos.
    /// </para>
    /// <para>
    /// <see cref="IsConfirmation"/> distingue el diálogo informativo (un botón "Aceptar")
    /// del diálogo de confirmación ("Sí"/"No") usado por <c>ConfirmAsync</c>.
    /// </para>
    /// </remarks>
    public sealed class MessageDialogModel
    {
        private MessageDialogModel(
            string caption,
            string heading,
            string message,
            string footnote,
            string expander,
            MessageType messageType,
            MessageIcon messageIcon,
            bool isConfirmation)
        {
            Caption = caption;
            Heading = heading;
            Message = message;
            Footnote = footnote;
            Expander = expander;
            MessageType = messageType;
            MessageIcon = messageIcon;
            IsConfirmation = isConfirmation;
        }

        /// <summary>Título de la ventana del diálogo.</summary>
        public string Caption { get; }

        /// <summary>Encabezado destacado del diálogo (fuente grande). Vacío = se omite la sección.</summary>
        public string Heading { get; }

        /// <summary>Mensaje principal del diálogo.</summary>
        public string Message { get; }

        /// <summary>Pie de página complementario. Vacío = se omite la sección.</summary>
        public string Footnote { get; }

        /// <summary>Detalles técnicos desplegables (pila de excepción, contexto). Vacío = se omite la sección.</summary>
        public string Expander { get; }

        /// <summary>Naturaleza del mensaje (informativo, advertencia, error, genérico).</summary>
        public MessageType MessageType { get; }

        /// <summary>Icono solicitado para el diálogo.</summary>
        public MessageIcon MessageIcon { get; }

        /// <summary>Indica si el diálogo pide confirmación ("Sí"/"No") en lugar de solo aceptar.</summary>
        public bool IsConfirmation { get; }

        /// <summary>Indica si debe renderizarse el encabezado de fuente grande.</summary>
        public bool ShowHeading => !string.IsNullOrEmpty(Heading);

        /// <summary>Indica si debe renderizarse el pie de página.</summary>
        public bool ShowFootnote => !string.IsNullOrEmpty(Footnote);

        /// <summary>Indica si debe renderizarse la sección desplegable de detalles técnicos.</summary>
        public bool ShowExpander => !string.IsNullOrEmpty(Expander);

        /// <summary>Indica si debe renderizarse el icono del diálogo.</summary>
        public bool ShowIcon => MessageIcon != MessageIcon.None;

        /// <summary>
        /// Proyecta los argumentos neutrales del servicio de mensajes hacia el modelo del
        /// diálogo WPF.
        /// </summary>
        /// <param name="args">Argumentos del mensaje a proyectar.</param>
        /// <param name="isConfirmation">
        /// <see langword="true"/> cuando el diálogo se muestra desde <c>ConfirmAsync</c>
        /// (botones "Sí"/"No"); <see langword="false"/> para el diálogo informativo.
        /// </param>
        /// <returns>Modelo inmutable listo para enlazar a <see cref="MessageDialogWindow"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Si <paramref name="args"/> es <see langword="null"/>.</exception>
        /// <example>
        /// <code>
        /// var model = MessageDialogModel.FromArgs(new MessageArgs
        /// {
        ///     Caption = "Error de lectura",
        ///     Message = ex.Message,
        ///     Expander = ex.ToString(),
        ///     MessageType = MessageType.Error,
        ///     MessageIcon = MessageIcon.Error
        /// });
        /// </code>
        /// </example>
        public static MessageDialogModel FromArgs(MessageArgs args, bool isConfirmation = false)
        {
            ArgumentNullException.ThrowIfNull(args);
            return new MessageDialogModel(
                args.Caption,
                args.Heading,
                args.Message,
                args.Footnote,
                args.Expander,
                args.MessageType,
                args.MessageIcon,
                isConfirmation);
        }
    }
}
