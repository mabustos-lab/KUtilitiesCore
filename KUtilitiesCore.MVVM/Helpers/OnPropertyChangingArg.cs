using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Helpers
{
    /// <summary>
    /// Representa los argumentos de evento para un cambio de propiedad, permitiendo validación y cancelación.
    /// </summary>
    public class PropertyChangingEventArgs : EventArgs
    {
        /// <summary>
        /// Crea una nueva instancia de PropertyChangingEventArgs con los detalles especificados de la propiedad.
        /// </summary>
        /// <param name="propertyName">El nombre de la propiedad que está cambiando.</param>
        /// <param name="oldValue">El valor anterior de la propiedad antes del cambio.</param>
        /// <param name="newValue">El nuevo valor de la propiedad después del cambio.</param>
        public PropertyChangingEventArgs(string propertyName, object? oldValue, object? newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
            Cancel = false;
        }

        /// <summary>
        /// Obtiene el nombre de la propiedad que está cambiando.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Obtiene el valor anterior de la propiedad antes del cambio.
        /// </summary>
        public object? OldValue { get; }

        /// <summary>
        /// Obtiene el nuevo valor de la propiedad después del cambio.
        /// </summary>
        public object? NewValue { get; }

        /// <summary>
        /// Establece un valor que indica si el cambio de propiedad debe cancelarse.
        /// </summary>
        public bool Cancel { get; set; }        
    }
}