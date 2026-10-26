using KUtilitiesCore.Data.Converter.Abstracts;
using System;

namespace KUtilitiesCore.Data.Converter.Types
{
    /// <summary>
    /// Convertidor especializado que transforma cadenas en valores de enumeración,
    /// aceptando tanto el nombre del miembro como su valor numérico subyacente.
    /// </summary>
    /// <typeparam name="TTargetType">Tipo de enumeración destino.</typeparam>
    public class EnumConverter<TTargetType> : NonNullableConverter<TTargetType>
            where TTargetType : struct, IConvertible
    {
        #region Fields

        private readonly Type enumType;
        private readonly bool ignoreCase;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa el convertidor ignorando mayúsculas/minúsculas al interpretar
        /// nombres de miembros de la enumeración.
        /// </summary>
        public EnumConverter()
            : this(true)
        {
        }

        /// <summary>
        /// Inicializa el convertidor especificando si la comparación de nombres de
        /// miembros de la enumeración ignora mayúsculas/minúsculas.
        /// </summary>
        /// <param name="ignoreCase">
        /// Indica si la interpretación de nombres de miembros debe ignorar
        /// mayúsculas/minúsculas.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Se produce cuando <typeparamref name="TTargetType"/> no es una enumeración.
        /// </exception>
        public EnumConverter(bool ignoreCase)
        {
            if (!typeof(TTargetType).IsEnum)
            {
                throw new ArgumentException(string.Format("Type {0} is not a valid Enum", enumType));
            }
            enumType = typeof(TTargetType);
            this.ignoreCase = ignoreCase;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convierte la cadena en un valor de la enumeración destino: primero intenta
        /// interpretarla como valor numérico definido y, si no lo es, como nombre de miembro.
        /// </summary>
        /// <param name="value">Cadena de origen a convertir.</param>
        /// <param name="result">
        /// Valor de enumeración resultante; <c>default</c> si la conversión no es posible.
        /// </param>
        /// <returns><c>true</c> si la conversión fue exitosa; <c>false</c> en caso contrario.</returns>
        protected override bool InternalConvert(string value, out TTargetType result)
        {
            int intValue = -1;
            if (int.TryParse(value, out intValue))
            {
                result = default;
                bool success = Enum.IsDefined(typeof(TTargetType), intValue);
                if (success)
                {
                    result = (TTargetType)Enum.ToObject(typeof(TTargetType), intValue);
                }
                return success;
            }
            return Enum.TryParse(value, ignoreCase, out result);
        }

        #endregion Methods
    }
}