﻿using System;
using System.Linq;

namespace KUtilitiesCore.OrderedInfo
{
    /// <summary>
    /// Información del ordenamiento de una propiedad
    /// </summary>
    public class OrderedQueryableInfo
    {
        #region Properties

        /// <summary>
        /// Dirección de ordenamiento de la propiedad
        /// </summary>
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Propiedad a ordenar
        /// </summary>
        public PNameInfo Property { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Crea una nueva instancia de <see cref="OrderedQueryableInfo"/>
        /// </summary>
        public OrderedQueryableInfo()
        {
        }

        #endregion Constructors
    }
}