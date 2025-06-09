using System;

namespace KUtilitiesCore.Data.Converter.Types
{
    /// <summary>
    /// Convierte un arreglo de cadenas en un arreglo de un tipo específico utilizando un convertidor interno.
    /// </summary>
    /// <typeparam name="TTargetType">Tipo de los elementos del arreglo de destino.</typeparam>
    public class ArrayConverter<TTargetType> : IArrayTypeConverter<TTargetType[]>
    {
        #region Fields

        /// <summary>
        /// Convertidor interno utilizado para convertir cada elemento individual.
        /// </summary>
        private readonly ITypeConverter<TTargetType> internalConverter;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ArrayConverter{TTargetType}"/>.
        /// </summary>
        /// <param name="internalConverter">Convertidor utilizado para convertir cada elemento del arreglo.</param>
        public ArrayConverter(ITypeConverter<TTargetType> internalConverter)
        {
            this.internalConverter = internalConverter;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Obtiene o establece el carácter separador utilizado para dividir la cadena de entrada.
        /// </summary>
        public char Separator { get; set; } = ',';

        /// <summary>
        /// Obtiene el tipo de destino al que se convierte la cadena.
        /// </summary>
        public Type TargetType => typeof(TTargetType[]);

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determina si el convertidor puede convertir el valor de texto especificado al tipo de destino.
        /// </summary>
        /// <param name="value">El valor de texto a evaluar.</param>
        /// <returns>True si el convertidor puede convertir el valor; de lo contrario, false.</returns>
        public virtual bool CanConvert(string value)
        {
            string[] values = value.Split(Separator);
            return TryConvert(values, out _);
        }

        /// <summary>
        /// Intenta convertir un arreglo de cadenas al tipo de colección de destino.
        /// </summary>
        /// <param name="values">Arreglo de cadenas a convertir.</param>
        /// <returns>El objeto convertido si la conversión es exitosa; de lo contrario, null.</returns>
        public virtual object? TryConvert(string[] values)
        {
            if (TryConvert(values, out TTargetType[] result))
            {
                return result as object;
            }
            return null;
        }

        /// <summary>
        /// Intenta convertir un arreglo de cadenas al arreglo de tipo de destino.
        /// </summary>
        /// <param name="value">Arreglo de cadenas a convertir.</param>
        /// <param name="result">Resultado de la conversión si es exitosa; valor por defecto en caso contrario.</param>
        /// <returns>True si la conversión fue exitosa; de lo contrario, false.</returns>
        public virtual bool TryConvert(string[] value, out TTargetType[] result)
        {
            result = new TTargetType[value.Length];

            for (int pos = 0; pos < value.Length; pos++)
            {
                if (!internalConverter.TryConvert(value[pos], out TTargetType element))
                    return false;

                result[pos] = element;
            }

            return true;
        }

        /// <summary>
        /// Intenta convertir el valor de texto especificado al tipo de destino.
        /// </summary>
        /// <param name="value">El valor de texto a convertir.</param>
        /// <returns>El objeto convertido si la conversión es exitosa; de lo contrario, null.</returns>
        public virtual object? TryConvert(string value)
        {
            return TryConvert(value.Split(Separator));
        }

        #endregion Methods
    }
}