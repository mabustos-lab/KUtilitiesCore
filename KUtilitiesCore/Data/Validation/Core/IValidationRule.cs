namespace KUtilitiesCore.Data.Validation.Core
{
    /// <summary>
    /// Interfaz para una regla de validación específica.
    /// </summary>
    /// <typeparam name="T">El tipo de objeto que se está validando.</typeparam>
    public interface IValidationRule<T>
    {
        #region Methods

        /// <summary>
        /// Ejecuta la regla de validación sobre la instancia dada.
        /// </summary>
        /// <param name="context">El contexto de validación.</param>
        /// <returns>Una colección de fallos de validación (vacía si la regla pasa).</returns>
        IEnumerable<ValidationFailure> Validate(ValidationContext<T> context);

        #endregion Methods
    }
}