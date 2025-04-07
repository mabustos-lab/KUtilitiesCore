namespace KUtilitiesCore.Validation.Core
{
    /// <summary>
    /// Contexto de validación que se pasa a las reglas.
    /// </summary>
    /// <typeparam name="T">El tipo de objeto que se está validando.</typeparam>
    public class ValidationContext<T>(T instanceToValidate)
    {
        #region Properties

        /// <summary>
        /// La instancia del objeto que se está validando.
        /// </summary>
        public T InstanceToValidate { get; } = instanceToValidate;

        #endregion Properties

        // Podría extenderse para incluir más información contextual si fuera necesario (ej:
        // servicios resueltos por DI).
    }
}