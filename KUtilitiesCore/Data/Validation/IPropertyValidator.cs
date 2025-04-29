using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Interfaz para un validador específico de una propiedad.
    /// </summary>
    internal interface IPropertyValidator<T, TProperty>
    {
        bool IsValid(ValidationContext<T> context, TProperty value);
        string GetErrorMessage(ValidationContext<T> context, TProperty value);
    }
}
