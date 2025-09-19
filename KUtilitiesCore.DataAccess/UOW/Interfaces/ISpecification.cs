using System.Linq.Expressions;

namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Define un contrato para el patrón de Especificación, permitiendo encapsular la lógica de
    /// consulta (filtrado, ordenación e inclusiones). La paginación se maneja externamente a través
    /// de IPagingOptions.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad al que se aplica la especificación.</typeparam>
    public interface ISpecification<T>
        where T : class
    {
        #region Properties

        /// <summary>
        /// Expresión de criterio para filtrar las entidades.
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// Lista de expresiones lambda para incluir propiedades de navegación.
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// Lista de cadenas para incluir propiedades de navegación (útil para includes anidados o dinámicos).
        /// </summary>
        List<string> IncludeStrings { get; }

        /// <summary>
        /// Expresión para ordenar los resultados en orden ascendente.
        /// </summary>
        Expression<Func<T, object>> OrderBy { get; }

        /// <summary>
        /// Expresión para ordenar los resultados en orden descendente.
        /// </summary>
        Expression<Func<T, object>> OrderByDescending { get; }

        #endregion Properties
    }
}