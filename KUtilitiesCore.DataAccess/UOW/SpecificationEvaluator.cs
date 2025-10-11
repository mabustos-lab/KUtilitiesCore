using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System.Data.Entity;

namespace KUtilitiesCore.DataAccess.UOW
{
    /// <summary>
    /// Clase responsable de aplicar una especificación (ISpecification) a un IQueryable.
    /// </summary>
    public static class SpecificationEvaluator<TEntity> where TEntity : class
    {
        #region Methods

        /// <summary>
        /// Aplica la especificación dada a la consulta IQueryable de entrada.
        /// </summary>
        public static IQueryable<TEntity> GetQuery(
            IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> specification)
        {
            var query = inputQuery;
            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);
            if (specification.Includes != null)
                query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
            if (specification.IncludeStrings != null)
                query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);
            return query;
        }

        #endregion Methods
    }
}