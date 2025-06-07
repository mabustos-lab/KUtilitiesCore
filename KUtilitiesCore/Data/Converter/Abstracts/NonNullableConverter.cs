namespace KUtilitiesCore.Data.Converter.Abstracts
{
    /// <summary>
    /// Clase base abstracta para convertir valores de texto a tipos no anulables.
    /// </summary>
    /// <typeparam name="TTargetType">Tipo de destino al que se convertirá el valor.</typeparam>
    public abstract class NonNullableConverter<TTargetType> : BaseConverter<TTargetType>
    {
        #region Methods

        /// <summary>
        /// Intenta convertir el valor de texto al tipo de destino.
        /// Si el valor es nulo o vacío, la conversión falla y se asigna el valor predeterminado.
        /// </summary>
        /// <param name="value">Valor de texto a convertir.</param>
        /// <param name="result">Resultado de la conversión.</param>
        /// <returns>True si la conversión fue exitosa; de lo contrario, false.</returns>
        public override bool TryConvert(string value, out TTargetType result)
        {
            // Sección crítica: Si el valor es nulo o vacío, no es posible convertir a un tipo no anulable.
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default!;
                return false;
            }

            return InternalConvert(value, out result);
        }

        /// <summary>
        /// Realiza la conversión interna del valor de texto al tipo de destino.
        /// Implementar la lógica específica de conversión en clases derivadas.
        /// </summary>
        /// <param name="value">Valor de texto a convertir.</param>
        /// <param name="result">Resultado de la conversión.</param>
        /// <returns>True si la conversión fue exitosa; de lo contrario, false.</returns>
        protected abstract bool InternalConvert(string value, out TTargetType result);

        #endregion Methods
    }
}