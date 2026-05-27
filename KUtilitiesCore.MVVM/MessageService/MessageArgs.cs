using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.MessageService
{
    /// <summary>
    /// Representa el contenido del mensaje
    /// </summary>
    public class MessageArgs
    {
        /// <summary>
        /// El título de la ventana del mensaje
        /// </summary>
        public string Caption { get; set; } = string.Empty;
        /// <summary>
        /// El título principal en la parte superior (opcional).
        /// Usa una fuente grande.
        /// </summary>
        public string Heading { get; set; } = string.Empty;
        /// <summary>
        /// El contenido del mensaje a mostrar.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Añade una nota al pie de página.
        /// </summary>
        public string Footnote {  get; set; } = string.Empty;
        /// <summary>
        /// Permite ocultar o mostrar texto técnico o detallado mediante un botón desplegable.
        /// </summary>
        public string Expander {  get; set; } = string.Empty;
        /// <summary>
        /// El tipo de mensaje (<see cref="MessageType"/>).
        /// </summary>
        public MessageType MessageType { get; set; } = MessageType.Generic;
        /// <summary>
        /// El icono de pregunta a mostrar (por defecto <see cref="MessageIcon.Question"/>).
        /// </summary>
        public MessageIcon MessageIcon { get; set; } = MessageIcon.None;
    }
}
