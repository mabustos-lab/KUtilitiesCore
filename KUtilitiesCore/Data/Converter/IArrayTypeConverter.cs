namespace KUtilitiesCore.Data.Converter
{
    /// <summary>
    /// Interfaz para convertir arreglos de cadenas a un tipo de colección específico.
    /// </summary>
    /// <typeparam name="TTargetType">Tipo de destino al que se convertirá el arreglo.</typeparam>
    public interface IArrayTypeConverter<TTargetType> : ITypeConverter
    {
        /// <summary>
        /// Caracter que se utiliza como separador al convertir cadenas a colecciones.
        /// </summary>
        char Separator { get; set; }

        /// <summary>
        /// Intenta convertir un arreglo de cadenas al tipo de colección de destino.
        /// </summary>
        /// <param name="value">Arreglo de cadenas a convertir.</param>
        /// <param name="result">Resultado de la conversión si es exitosa; valor por defecto en caso contrario.</param>
        /// <returns>True si la conversión fue exitosa; de lo contrario, false.</returns>
        bool TryConvert(string[] value, out TTargetType result);
    }
}