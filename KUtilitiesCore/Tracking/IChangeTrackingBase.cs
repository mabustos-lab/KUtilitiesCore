using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KUtilitiesCore.Tracking
{
    /// <summary>
    /// Interfaz que implementa IEditableObject para gestionar el seguimiento y reversión de cambios.
    /// </summary>
    public interface IChangeTrackingBase:IEditableObject
    {
        /// <summary>
        /// Indica si se han modificado los valores de las propiedades después de invocar BeginEdit.
        /// </summary>
        [NotMapped]
        [Display(AutoGenerateField = false)]
        bool IsChanged { get; }
    }
}