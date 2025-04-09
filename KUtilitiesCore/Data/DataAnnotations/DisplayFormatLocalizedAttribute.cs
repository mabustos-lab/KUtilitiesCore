using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KUtilitiesCore.Data.DataAnnotations
{
    /// <summary>
    /// Especifica el modo en que los datos se muestran
    /// </summary>
    public class DisplayFormatLocalizedAttribute : DisplayFormatAttribute
    {
        #region Constructors
        public DisplayFormatLocalizedAttribute(string diplayformat) { DataFormatString = diplayformat; }
        #endregion Constructors

        #region Properties

        /// <summary>
        /// Representa el tipo del Recurso donde se buscará el texto para la FormatString, FormatString representa la
        /// propiedad de Acceso al recurso, si este no es nulo.
        /// </summary>
        public Type ResourceType { get; set; }
        #endregion Properties

        #region Methods
        public string GetDataFormatString()
        {
            if (ResourceType != null)
            {
                return Helpers.ResourceHelpers.GetFromResource(ResourceType, c => c.GetString(DataFormatString));
            }
            return DataFormatString;
        }
        #endregion Methods
    }
}