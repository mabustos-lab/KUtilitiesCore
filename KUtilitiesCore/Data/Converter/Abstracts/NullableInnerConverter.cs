namespace KUtilitiesCore.Data.Converter.Abstracts
{
    /// <summary>
    /// Clase base abstracta para convertir valores de texto a tipos anulables (nullable) usando un convertidor interno para el tipo no anulable.
    /// </summary>
    /// <typeparam name="TTargetType">Tipo de valor estructural (struct) que será convertido.</typeparam>
    public abstract class NullableInnerConverter<TTargetType> : NullableConverter<TTargetType?>
            where TTargetType : struct
    {
        /// <summary>
        /// Convertidor interno para el tipo no anulable.
        /// Se utiliza para realizar la conversión real del valor cuando no es nulo.
        /// </summary>
        private readonly NonNullableConverter<TTargetType> internalConverter;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="NullableInnerConverter{TTargetType}"/>.
        /// </summary>
        /// <param name="internalConverter">Convertidor para el tipo no anulable.</param>
        public NullableInnerConverter(NonNullableConverter<TTargetType> internalConverter)
        {
            this.internalConverter = internalConverter;
        }

        /// <summary>
        /// Realiza la conversión interna del valor de texto al tipo anulable.
        /// </summary>
        /// <param name="value">Valor de texto a convertir.</param>
        /// <param name="result">Resultado de la conversión, o null si falla.</param>
        /// <returns>True si la conversión fue exitosa; de lo contrario, false.</returns>
        /// <remarks>
        /// Sección crítica: Utiliza el convertidor interno para intentar la conversión y encapsula el resultado en un tipo anulable.
        /// </remarks>
        protected override bool InternalConvert(string value, out TTargetType? result)
        {
            result = default;

            TTargetType innerConverterResult;

            if (internalConverter.TryConvert(value, out innerConverterResult))
            {
                result = innerConverterResult;

                return true;
            }

            return false;
        }
    }
}