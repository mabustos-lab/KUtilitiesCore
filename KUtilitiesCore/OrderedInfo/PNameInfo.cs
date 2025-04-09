using System;
using System.Linq;

namespace KUtilitiesCore.OrderedInfo
{
    /// <summary>
    /// Encapsula la información de una propiedad con su nombre y texto mostrado al usuario
    /// </summary>
    public class PNameInfo
    {
        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="PNameInfo"/> con los nombres proporcionados
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <param name="displayName">Nombre(display) mostrado al usuario</param>
        public PNameInfo(string propertyName, string displayName)
        {
            PropertyName = propertyName;
            DisplayName = displayName;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Nombre de la propiedad mostrado al usuario
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Nombre de la propiedad
        /// </summary>
        public string PropertyName { get; set; }

        #endregion Properties
    }
}