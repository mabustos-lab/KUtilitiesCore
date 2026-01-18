using KUtilitiesCore.DataAccess.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Contrato base para el patrón de Especificación.
    /// Soporta Parametrización (SQL Nativo).
    /// </summary>
    public interface ISpecificationBase: IPagingOptions
    {
        /// <summary>
        /// Diccionario de parámetros para repositorios SQL Nativo o Stored Procedures.
        /// Permite mapear valores sin tener que parsear el Expression Tree.
        /// Clave: Nombre del parámetro (ej. "Id"), Valor: El valor.
        /// </summary>
        IDictionary<string, object> Parameters { get; }
    }

    /// <summary>
    /// Contrato para el patrón de Especificación.
    /// Soporta tanto LINQ (EF Core) como Parametrización (SQL Nativo).
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    public interface ISpecification<T>:ISpecificationBase
    {
        /// <summary>
        /// La expresión lógica (predicado) para ORMs con soporte LINQ (EF Core).
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }
        /// <summary>
        /// Lista de expresiones para incluir entidades relacionadas (JOINs / Include).
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }
        /// <summary>
        /// Lista de "cadenas" para includes (útil para propiedades anidadas).
        /// </summary>
        List<string> IncludeStrings { get; }
        /// <summary>
        /// Expresión para ordenamiento ascendente.
        /// </summary>
        Expression<Func<T, object>> OrderBy { get; }

        /// <summary>
        /// Expresión para ordenamiento descendente.
        /// </summary>
        Expression<Func<T, object>> OrderByDescending { get; }
        /// <summary>
        /// Indica si la consulta debe realizarse sin rastreo de cambios (AsNoTracking).
        /// Ideal para escenarios de solo lectura.
        /// </summary>
        bool IsAsNoTracking { get; }
    }
}
