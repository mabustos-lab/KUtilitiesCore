using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Define una interfaz para proporcionar acceso al contenido de un documento en un ViewModel.
    /// </summary>
    public interface IViewModelDocumentContent
    {
        /// <summary>
        /// Obtiene o establece el servicio que actúa como propietario del documento actual.
        /// </summary>
        [Display(AutoGenerateField = false)]
        IViewModelDocumentOwner DocumentOwner { get; set; }

        /// <summary>
        /// Obtiene el título del documento.
        /// Este título puede ser utilizado para mostrar información descriptiva del documento en la interfaz de usuario.
        /// </summary>
        [Display(AutoGenerateField = false)]
        object Title { get; }

        /// <summary>
        /// Método invocado antes de que el documento sea cerrado (ocultado).
        /// Permite prevenir el cierre del documento mediante la manipulación de los datos del evento.
        /// </summary>
        /// <param name="e">Datos del evento que permiten prevenir operaciones específicas en el documento.</param>
        [Display(AutoGenerateField = false)]
        void OnClose(CancelEventArgs e);

        /// <summary>
        /// Método invocado después de que el documento haya sido cerrado (ocultado).
        /// Este método puede ser utilizado para liberar recursos o realizar tareas de limpieza.
        /// </summary>
        [Display(AutoGenerateField = false)]
        void OnDestroy();
    }
}
