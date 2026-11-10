using KUtilitiesCore.MVVM.MessageService;
using System.Windows;
using System.Windows.Media;

namespace KUtilitiesCore.Data.WPF.MessageService
{
    /// <summary>
    /// Diálogo de mensajes propio de WPF con anatomía TaskDialog: encabezado de fuente
    /// grande, mensaje principal, sección desplegable de detalles técnicos, pie de página
    /// e icono según <see cref="MessageIcon"/>. Los botones se adaptan al modo: un único
    /// "Aceptar" para mensajes informativos o "Sí"/"No" para confirmaciones.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Se implementa un diálogo XAML propio en lugar de <c>Microsoft.Win32.TaskDialog</c>
    /// porque ese tipo reside en <c>System.Windows.Forms.dll</c> y exigiría referenciar
    /// Windows Forms, prohibido por la especificación del asistente WPF. La alternativa
    /// de invocar la API nativa <c>TaskDialogIndirect</c> vía P/Invoke también se descartó
    /// por el costo de interoperabilidad y de pruebas frente al control total de estilos
    /// que ofrece un diálogo XAML.
    /// </para>
    /// <para>
    /// Toda la configuración visual se deriva de <see cref="MessageDialogModel"/>, de modo
    /// que el mapeo (qué secciones se omiten, qué icono y botones se muestran) es verificable
    /// sin instanciar la ventana.
    /// </para>
    /// </remarks>
    public partial class MessageDialogWindow : Window
    {
        /// <summary>
        /// Inicializa el diálogo a partir del modelo proyectado y adapta sus secciones
        /// y botones según el contenido.
        /// </summary>
        /// <param name="model">Modelo con el contenido y el modo del diálogo.</param>
        /// <exception cref="System.ArgumentNullException">Si <paramref name="model"/> es <see langword="null"/>.</exception>
        public MessageDialogWindow(MessageDialogModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            InitializeComponent();

            Title = ResolveTitle(model);
            HeadingText.Text = model.Heading;
            HeadingText.Visibility = model.ShowHeading ? Visibility.Visible : Visibility.Collapsed;

            MessageText.Text = model.Message;
            MessageText.Visibility = string.IsNullOrEmpty(model.Message)
                ? Visibility.Collapsed
                : Visibility.Visible;

            ExpanderText.Text = model.Expander;
            DetailsExpander.Visibility = model.ShowExpander ? Visibility.Visible : Visibility.Collapsed;

            FootnoteText.Text = model.Footnote;
            FootnoteText.Visibility = model.ShowFootnote ? Visibility.Visible : Visibility.Collapsed;

            ApplyIcon(model);

            OkButton.Visibility = model.IsConfirmation ? Visibility.Collapsed : Visibility.Visible;
            ConfirmButton.Visibility = model.IsConfirmation ? Visibility.Visible : Visibility.Collapsed;
            CancelButton.Visibility = model.IsConfirmation ? Visibility.Visible : Visibility.Collapsed;
        }

        private static string ResolveTitle(MessageDialogModel model)
        {
            if (!string.IsNullOrEmpty(model.Caption))
                return model.Caption;
            if (model.ShowHeading)
                return model.Heading;
            return "Mensaje";
        }

        private void ApplyIcon(MessageDialogModel model)
        {
            if (!model.ShowIcon)
            {
                IconGlyph.Visibility = Visibility.Collapsed;
                return;
            }

            (string glyph, Brush brush) = model.MessageIcon switch
            {
                MessageIcon.Information => ("\u2139", Brushes.DodgerBlue),
                MessageIcon.Warning => ("\u26A0", Brushes.Orange),
                MessageIcon.Error => ("\u2716", Brushes.Red),
                MessageIcon.Question => ("?", Brushes.DodgerBlue),
                _ => (string.Empty, Brushes.Black)
            };

            IconGlyph.Text = glyph;
            IconGlyph.Foreground = brush;
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e) => DialogResult = true;

        private void OnCancelClick(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
