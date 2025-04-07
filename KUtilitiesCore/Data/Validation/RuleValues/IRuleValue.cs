namespace KUtilitiesCore.Data.Validation.RuleValues
{
    /// <summary>
    /// Interface base de la regla de valores permitidos
    /// </summary>
    public interface IRuleValue
    {
        #region Properties

        bool HasRule { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Obtiene una descripción legible de los valores permitidos.
        /// </summary>
        /// <returns>Una cadena que describe los valores permitidos.</returns>
        string GetAllowedDescription();

        /// <summary>
        /// Verifica si el valor proporcionado está dentro del conjunto permitido.
        /// </summary>
        /// <param name="value">El valor a verificar.</param>
        /// <returns>True si el valor está permitido, False en caso contrario.</returns>
        bool IsAllowed(object value);

        #endregion Methods
    }
}