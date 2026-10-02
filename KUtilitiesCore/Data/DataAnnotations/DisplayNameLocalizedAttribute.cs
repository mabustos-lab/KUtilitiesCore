using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataAnnotations
{

    /// <summary>
    /// Extension de <see cref="DisplayNameAttribute"/> que permite extraer el texto de un Recurso
    /// </summary>
    [AttributeUsage( AttributeTargets.Property)]
    public class DisplayNameLocalizedAttribute : DisplayNameAttribute
    {
        #region Constructors
        /// <summary>
        /// Inicializa el atributo con el nombre para mostrar literal o la clave de
        /// recurso a localizar, según se configure <see cref="ResourceType"/>.
        /// </summary>
        /// <param name="diplayName">Nombre para mostrar literal o clave de recurso.</param>
        public DisplayNameLocalizedAttribute(string diplayName) : base(diplayName)
        {
        }
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
        /// Resuelve el nombre para mostrar final: si <see cref="ResourceType"/> está definido,
        /// busca la clave en el recurso; en caso contrario devuelve el nombre literal.
        /// </summary>
        /// <returns>El nombre para mostrar localizado o literal.</returns>
        public string GetDisplayName()
        {
            if(ResourceType is not null)
            {
                return Helpers.ResourceHelpers.GetFromResource(ResourceType, c => c.GetString(base.DisplayName))??string.Empty;
            }
            return base.DisplayName;
        }
        #endregion Methods
    }
}