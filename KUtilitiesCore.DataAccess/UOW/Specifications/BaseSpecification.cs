using KUtilitiesCore.DataAccess.Paging;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.UOW.Specifications
{
    public abstract class BaseSpecification : PagingOptions, ISpecificationBase
    {
        /// <inheritdoc/>
        public virtual IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
    }

    public abstract class BaseSpecification<T> : BaseSpecification, ISpecification<T>
    {
        /// <inheritdoc/>
        public virtual Expression<Func<T, bool>> Criteria { get; }
        /// <inheritdoc/>
        public virtual List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
        /// <inheritdoc/>
        public virtual List<string> IncludeStrings { get; } = new List<string>();
        /// <inheritdoc/>
        public virtual Expression<Func<T, object>> OrderBy { get; private set; }
        /// <inheritdoc/>
        public virtual Expression<Func<T, object>> OrderByDescending { get; private set; }
        /// <inheritdoc/>
        public bool IsAsNoTracking { get; private set; }
        /// <summary>
        /// Constructor sin criterio (útil para obtener todos o solo usar parámetros SQL)
        /// </summary>
        protected BaseSpecification()
        {
        }

        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }
        protected virtual void AddParameter(string name, object value)
            => Parameters[name] = value;
        protected virtual void AddInclude(string includeString)
        => IncludeStrings.Add(includeString);
        protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
         => OrderBy = orderByExpression;
        protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        => OrderByDescending = orderByDescendingExpression;
        protected virtual void ApplyNoTracking()
        => IsAsNoTracking = true;
    }
    /// <summary>
    /// Especificación para buscar una entidad por una condición de identidad.
    /// </summary>
    public class EntityByIdSpecification<T> : BaseSpecification<T>, ISpecification<T> where T : class
    {
        public EntityByIdSpecification(Expression<Func<T, bool>> idPredicate)
            : base(idPredicate)
        {
        }
    }
}
