using System;

namespace KUtilitiesCore.Data.Converter.Abstracts
{
    /// <summary>
    /// Clase base abstracta que implementa la interfaz <see cref="ITypeConverter{TTargetType}"/> para convertir un texto a un tipo específico.
    /// </summary>
    /// <typeparam name="TTargetType">Tipo destino al que se desea convertir la cadena de texto.</typeparam>
    public abstract class BaseConverter<TTargetType> : ITypeConverter<TTargetType>
    {
        #region Propiedades

        /// <summary>
        /// Obtiene el tipo de destino al que se convierte el texto.
        /// </summary>
        public virtual Type TargetType => typeof(TTargetType);

        #endregion Propiedades

        #region Métodos

        /// <summary>
        /// Determina si el convertidor puede convertir el valor de texto especificado al tipo de destino.
        /// </summary>
        /// <param name="value">El valor de texto a evaluar.</param>
        /// <returns>True si el convertidor puede convertir el valor; caso contrario, false.</returns>
        public virtual bool CanConvert(string value)
            => TryConvert(value, out TTargetType _);

        /// <summary>
        /// Intenta convertir el valor de texto especificado al tipo de destino.
        /// </summary>
        /// <param name="value">El valor de texto a convertir.</param>
        /// <param name="result">El resultado de la conversión si es exitosa; caso contrario, el valor por defecto del tipo.</param>
        /// <returns>True si la conversión es exitosa; caso contrario, false.</returns>
        public abstract bool TryConvert(string value, out TTargetType result);

        /// <summary>
        /// Intenta convertir el valor de texto especificado al tipo de destino, llamando a <see cref="TryConvert(string, out TTargetType)"/>.
        /// </summary>
        /// <param name="value">El valor de texto a convertir.</param>
        /// <returns>El objeto convertido si la conversión es exitosa; caso contrario, null.</returns>
        public virtual object? TryConvert(string value)
            => TryConvert(value, out TTargetType result) ? result : null;

        #endregion Métodos
    }
}