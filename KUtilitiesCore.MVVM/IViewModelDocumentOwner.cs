using System;
using System.Linq;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Define una interfaz para proporcionar acceso al propietario del documento (servicio) en el nivel de documento.
    /// </summary>
    public interface IViewModelDocumentOwner
    {
        /// <summary>
        /// Cierra el documento especificado.
        /// </summary>
        /// <param name="documentContent">Instancia de <see cref="IViewModelDocumentContent"/> que representa el documento a cerrar.</param>
        /// <param name="force">Indica si se debe deshabilitar la lógica de confirmación. true para forzar el cierre; de lo contrario, false.</param>
        void Close(IViewModelDocumentContent documentContent, bool force = true);

        /// <summary>
        /// Obtiene o establece el contenido del contenedor del documento.
        /// </summary>
        IViewModelDocumentContent Content { get; set; }
    }
}
