using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KUtilitiesCore.Data.DataAnnotations
{
    /// <summary>
    /// Especifica el modo en que los datos se muestran
    /// </summary>
    [AttributeUsage( AttributeTargets.Property)]
    public class DisplayFormatLocalizedAttribute : DisplayFormatAttribute
    {
        #region Constructors
        /// <summary>
        /// Inicializa el atributo con la cadena de formato literal o la clave de recurso
        /// a localizar, según se configure <see cref="ResourceType"/>.
        /// </summary>
        /// <param name="diplayformat">Cadena de formato literal o clave de recurso.</param>
        public DisplayFormatLocalizedAttribute(string diplayformat) 
        { DataFormatString = diplayformat; }
        #endregion Constructors

        #region Properties

        /// <summary>
        /// Representa el tipo del Recurso donde se buscará el texto para la FormatString, FormatString representa la
        /// propiedad de Acceso al recurso, si este no es nulo.
        /// </summary>
        public Type? ResourceType { get; set; }
        #endregion Properties
        
        #region Methods
        /// <summary>
        /// Resuelve la cadena de formato final: si <see cref="ResourceType"/> está definido,
        /// busca la clave en el recurso; en caso contrario devuelve la cadena de formato literal.
        /// </summary>
        /// <returns>La cadena de formato localizada o literal.</returns>
        public string GetDataFormatString()
        {
            if (ResourceType is not null)
            {
                return Helpers.ResourceHelpers.GetFromResource(ResourceType, c => c.GetString(DataFormatString!))??string.Empty;
            }
            return DataFormatString!;
        }
        #endregion Methods
    }
}