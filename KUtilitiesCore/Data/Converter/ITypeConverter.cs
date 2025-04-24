using System;

namespace KUtilitiesCore.Data.Converter
{
    /// <summary>
    /// Interfaz base para convertir un texto a un tipo.
    /// </summary>
    public interface ITypeConverter
    {
        #region Propiedades

        /// <summary>
        /// Obtiene el tipo de destino al que se convierte el texto.
        /// </summary>
        Type TargetType { get; }

        #endregion Propiedades

        #region Métodos

        /// <summary>
        /// Determina si el convertidor puede convertir el valor de texto especificado al tipo de destino.
        /// </summary>
        /// <param name="value">El valor de texto a evaluar.</param>
        /// <returns>True si el convertidor puede convertir el valor; caso contrario, false.</returns>
        bool CanConvert(string value);

        /// <summary>
        /// Intenta convertir el valor de texto especificado al tipo de destino.
        /// </summary>
        /// <param name="value">El valor de texto a convertir.</param>
        /// <returns>El objeto convertido si la conversión es exitosa; caso contrario, null.</returns>
        object TryConvert(string value);

        #endregion Métodos
    }

    /// <summary>
    /// Interfaz que convierte un texto a un tipo específico.
    /// </summary>
    /// <typeparam name="TTargetType">Tipo destino al que se desea convertir la cadena de texto.</typeparam>
    public interface ITypeConverter<TTargetType> : ITypeConverter
    {
        #region Métodos

        /// <summary>
        /// Intenta convertir el valor de texto especificado al tipo de destino.
        /// </summary>
        /// <param name="value">El valor de texto a convertir.</param>
        /// <param name="result">
        /// El resultado de la conversión si es exitosa; caso contrario, el valor por defecto del tipo.
        /// </param>
        /// <returns>True si la conversión es exitosa; caso contrario, false.</returns>
        bool TryConvert(string value, out TTargetType result);

        #endregion Métodos
    }
}