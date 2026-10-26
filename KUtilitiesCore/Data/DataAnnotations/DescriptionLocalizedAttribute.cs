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

        /// <summary>
        /// Inicializa el atributo con la descripción literal o la clave de recurso a
        /// localizar, según se configure <see cref="ResourceType"/>.
        /// </summary>
        /// <param name="description">Descripción literal o clave de recurso.</param>
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

        /// <summary>
        /// Resuelve la descripción final: si <see cref="ResourceType"/> está definido,
        /// busca la clave en el recurso; en caso contrario devuelve la descripción literal.
        /// </summary>
        /// <returns>La descripción localizada o literal.</returns>
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
