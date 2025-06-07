using System;
using System.Linq;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Define una interfaz para proporcionar soporte en la asignación de un objeto padre al ViewModel.
    /// </summary>
    public interface ISupportParentViewModel
    {
        /// <summary>
        /// Evento que se dispara cuando el ViewModel padre ha cambiado.
        /// </summary>
        event EventHandler ParentViewModelChanged;

        /// <summary>
        /// Obtiene o establece el objeto que actúa como el ViewModel padre.
        /// Este objeto puede ser utilizado para establecer relaciones jerárquicas entre ViewModels.
        /// </summary>
        object? ParentViewModel { get; set; }
    }
}
