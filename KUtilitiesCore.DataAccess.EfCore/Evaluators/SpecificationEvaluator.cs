using System.Linq;
using Microsoft.EntityFrameworkCore;
using KUtilitiesCore.DataAccess.UOW.Interfaces;

namespace KUtilitiesCore.DataAccess.EfCore.Evaluators
{
    /// <summary>
    /// Componente encargado de interpretar la Especificación y aplicarla a un IQueryable de Entity Framework.
    /// Transforma las reglas de negocio (Expressions) en consultas SQL.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    public class SpecificationEvaluator<T> where T : class
    {
        /// <summary>
        /// Genera una consulta IQueryable aplicando todos los criterios definidos en la especificación.
        /// </summary>
        /// <param name="inputQuery">La consulta base (generalmente dbContext.Set<T>()).</param>
        /// <param name="specification">Las reglas a aplicar.</param>
        /// <returns>La consulta resultante lista para ser ejecutada.</returns>
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            var query = inputQuery;

            // 0. Aplicar No Tracking si se solicita (Optimización de lectura)
            if (specification.IsAsNoTracking)
            {
                query = query.AsNoTracking();
            }

            // 1. Aplicar Criterio de Filtrado (WHERE)
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // 2. Aplicar Inclusiones de Entidades Relacionadas (JOINs tipados)
            // Ejemplo: .Include(x => x.Orders)
            query = specification.Includes.Aggregate(query,
                                    (current, include) => current.Include(include));

            // 3. Aplicar Inclusiones por String (útil para niveles profundos o dinámicos)
            // Ejemplo: .Include("Orders.Items")
            query = specification.IncludeStrings.Aggregate(query,
                                    (current, include) => current.Include(include));

            // 4. Aplicar Ordenamiento
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // 5. Aplicar Paginación
            // Nota: Se aplica al final para asegurar que el OFFSET/FETCH se haga sobre los datos ya filtrados y ordenados.
            if (specification.SkipPagination)
            {
                int skip = (specification.PageNumber - 1) * specification.PageSize;
                query = query.Skip(skip).Take(specification.PageSize);
            }

            return query;
        }
    }
}
