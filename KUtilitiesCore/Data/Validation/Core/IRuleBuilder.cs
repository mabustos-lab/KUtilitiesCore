using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Validation.Core
{
    /// <summary>
    /// Interfaz para construir reglas de validación para una propiedad específica.
    /// </summary>
    /// <typeparam name="T">El tipo del objeto raíz.</typeparam>
    /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
    public interface IRuleBuilder<T, TProperty>
    {
        // Aquí irían las definiciones de métodos fluidos (NotNull, NotEmpty, Must, etc.)
        // Se implementarán como métodos de extensión para mantener esta interfaz limpia.
    }
}
