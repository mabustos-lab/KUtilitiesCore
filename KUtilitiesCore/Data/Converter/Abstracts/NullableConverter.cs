namespace KUtilitiesCore.Data.Converter.Abstracts
{
    /// <summary>
    /// Clase base abstracta para convertir valores de texto a tipos anulables.
    /// </summary>
    /// <typeparam name="TTargetType">Tipo de destino al que se convertirá el valor.</typeparam>
    public abstract class NullableConverter<TTargetType> : BaseConverter<TTargetType>
    {
        #region Methods

        /// <summary>
        /// Intenta convertir el valor de texto al tipo de destino.
        /// Si el valor es nulo o vacío, retorna el valor predeterminado y considera la conversión exitosa.
        /// </summary>
        /// <param name="value">Valor de texto a convertir.</param>
        /// <param name="result">Resultado de la conversión.</param>
        /// <returns>True si la conversión fue exitosa o el valor es nulo/vacío; de lo contrario, false.</returns>
        public override bool TryConvert(string value, out TTargetType result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default!;
                return true;
            }

            return InternalConvert(value, out result);
        }

        /// <summary>
        /// Realiza la conversión interna del valor de texto al tipo de destino.
        /// Sección crítica: Implementar la lógica específica de conversión en clases derivadas.
        /// </summary>
        /// <param name="value">Valor de texto a convertir.</param>
        /// <param name="result">Resultado de la conversión.</param>
        /// <returns>True si la conversión fue exitosa; de lo contrario, false.</returns>
        protected abstract bool InternalConvert(string value, out TTargetType result);

        #endregion Methods
    }
}