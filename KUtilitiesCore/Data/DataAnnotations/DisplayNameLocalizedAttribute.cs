﻿using System;
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