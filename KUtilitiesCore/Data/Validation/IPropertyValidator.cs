using KUtilitiesCore.Data.Validation.Core;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Interfaz para un validador específico de una propiedad.
    /// </summary>
    internal interface IPropertyValidator<T, TProperty>
    {
        #region Methods

        string GetErrorMessage(ValidationContext<T> context, TProperty value);

        bool IsValid(ValidationContext<T> context, TProperty value);

        #endregion Methods
    }
}