namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Representa las opciones de configuración para los procesos de validación.
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>
        /// Obtiene o establece el modo de validación para determinar cómo se aplican las reglas de validación.
        /// El valor predeterminado es <see cref="ValidationMode.CascadeMode"/>.
        /// </summary>
        public ValidationMode CascadeMode { get; set; } = ValidationMode.CascadeMode;

        /// <summary>
        /// Constructor privado para aplicar el patrón singleton.
        /// </summary>
        private ValidationOptions() { }

        /// <summary>
        /// Instancia singleton cargada de forma diferida de <see cref="ValidationOptions"/>.
        /// </summary>
        private static Lazy<ValidationOptions> instance = new Lazy<ValidationOptions>(() => new ValidationOptions());

        /// <summary>
        /// Obtiene la instancia singleton de <see cref="ValidationOptions"/>.
        /// </summary>
        public static ValidationOptions Instance => instance.Value;
    }
    /// <summary>
    /// Especifica los modos de validación que se pueden aplicar.
    /// </summary>
    public enum ValidationMode
    {
        /// <summary>
        /// La validación continúa para todas las reglas, incluso si algunas fallan.
        /// </summary>
        CascadeMode,

        /// <summary>
        /// La validación se detiene en la primera regla que falle.
        /// </summary>
        StopOnFirstFailure
    }
}