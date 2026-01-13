using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Validation
{

    /// <summary>
    /// Interfaz inicial del builder (para asegurar que RuleFor sea el inicio)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public interface IRuleBuilderInitial<T, TProperty> : IRuleBuilder<T, TProperty> { }
}
