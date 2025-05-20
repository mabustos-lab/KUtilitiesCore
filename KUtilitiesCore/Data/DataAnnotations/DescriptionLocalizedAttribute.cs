using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataAnnotations
{
    /// <summary>
    /// Especifica una Descripcion para una propiedad o evento localizable
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DescriptionLocalizedAttribute : DescriptionAttribute
    {
        #region Constructors

        public DescriptionLocalizedAttribute(string description)
            : base(description)
        { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Representa el tipo del Recurso donde se buscará el texto para la Descripción, la
        /// Description representa la propiedad de Aceso al recurso, si este no es nulo.
        /// </summary>
        public Type? ResourceType
        { get; set; }

        #endregion Properties

        #region Methods

        public string GetDescription()
        {
            if (ResourceType is not null)
            {
                return Helpers.ResourceHelpers.GetFromResource(ResourceType, c => c.GetString(base.Description))??string.Empty;
            }
            return base.Description;
        }

        #endregion Methods
    }
}
