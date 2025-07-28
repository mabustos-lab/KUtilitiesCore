using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System.Linq.Expressions;

namespace KUtilitiesCore.DataAccess.UOW
{
    /// <summary>
    /// Clase base para implementaciones de ISpecification. Proporciona una estructura común para
    /// definir especificaciones.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    public class Specification<T> : ISpecification<T>
        where T : class
    {
        #region Constructors

        /// <summary>
        /// Constructor base para especificaciones.
        /// </summary>
        /// <param name="criteria">La expresión de criterio inicial (opcional).</param>
        protected Specification(Expression<Func<T, bool>> criteria = null)
        { Criteria = criteria; }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public virtual Expression<Func<T, bool>> Criteria { get; protected set; }

        /// <inheritdoc/>
        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        /// <inheritdoc/>
        public List<string> IncludeStrings { get; } = new List<string>();

        /// <inheritdoc/>
        public Expression<Func<T, object>> OrderBy { get; private set; }

        /// <inheritdoc/>
        public Expression<Func<T, object>> OrderByDescending { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Combina esta especificación con otra usando un operador AND lógico.
        /// </summary>
        /// <param name="other">La otra especificación a combinar.</param>
        /// <returns>Una nueva especificación que representa la combinación AND.</returns>
        public ISpecification<T> And(ISpecification<T> other)
        { return new AndSpecification<T>(this, other); }

        /// <summary>
        /// Niega esta especificación.
        /// </summary>
        /// <returns>Una nueva especificación que representa la negación de la actual.</returns>
        public ISpecification<T> Not()
        { return new NotSpecification<T>(this); }

        /// <summary>
        /// Combina esta especificación con otra usando un operador OR lógico.
        /// </summary>
        /// <param name="other">La otra especificación a combinar.</param>
        /// <returns>Una nueva especificación que representa la combinación OR.</returns>
        public ISpecification<T> Or(ISpecification<T> other)
        { return new OrSpecification<T>(this, other); }

        /// <summary>
        /// Añade una expresión de inclusión de propiedad de navegación.
        /// </summary>
        /// <param name="includeExpression">La expresión de inclusión.</param>
        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        { Includes.Add(includeExpression); }

        /// <summary>
        /// Añade una cadena de inclusión de propiedad de navegación.
        /// </summary>
        /// <param name="includeString">La cadena de inclusión (ej. "Orders.OrderItems").</param>
        protected virtual void AddInclude(string includeString)
        { IncludeStrings.Add(includeString); }

        /// <summary>
        /// Aplica la ordenación ascendente.
        /// </summary>
        /// <param name="orderByExpression">La expresión de ordenación.</param>
        protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
            OrderByDescending = null; // Asegurar que solo uno esté activo
        }

        /// <summary>
        /// Aplica la ordenación descendente.
        /// </summary>
        /// <param name="orderByDescendingExpression">La expresión de ordenación.</param>
        protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
            OrderBy = null; // Asegurar que solo uno esté activo
        }

        #endregion Methods
    }

    /// <summary>
    /// Especificación para combinar dos especificaciones con un operador AND.
    /// </summary>
    internal class AndSpecification<T> : Specification<T>
        where T : class
    {
        #region Fields

        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        #endregion Fields

        #region Constructors

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _left = left ?? throw new ArgumentNullException(nameof(left));

            // Combinar criterios
            var paramExpr = Expression.Parameter(typeof(T));
            var exprBody = Expression.AndAlso(
                Expression.Invoke(left.Criteria, paramExpr),
                Expression.Invoke(right.Criteria, paramExpr));
            Criteria = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

            // Combinar Includes (evitando duplicados)
            Includes.AddRange(left.Includes.Union(right.Includes));
            IncludeStrings.AddRange(left.IncludeStrings.Union(right.IncludeStrings));
            if (left.OrderBy != null)
                ApplyOrderBy(left.OrderBy);
            else if (left.OrderByDescending != null)
                ApplyOrderByDescending(left.OrderByDescending);
            else if (right.OrderBy != null)
                ApplyOrderBy(right.OrderBy);
            else if (right.OrderByDescending != null)
                ApplyOrderByDescending(right.OrderByDescending);
        }

        #endregion Constructors
    }

    /// <summary>
    /// Especificación para negar otra especificación.
    /// </summary>
    internal class NotSpecification<T> : Specification<T>
        where T : class
    {
        #region Fields

        private readonly ISpecification<T> _original;

        #endregion Fields

        #region Constructors

        public NotSpecification(ISpecification<T> original)
        {
            _original = original ?? throw new ArgumentNullException(nameof(original));

            var paramExpr = Expression.Parameter(typeof(T));
            var exprBody = Expression.Not(Expression.Invoke(original.Criteria, paramExpr));
            Criteria = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

            Includes.AddRange(original.Includes);
            IncludeStrings.AddRange(original.IncludeStrings);
            if (original.OrderBy != null)
                ApplyOrderBy(original.OrderBy);
            if (original.OrderByDescending != null)
                ApplyOrderByDescending(original.OrderByDescending);
        }

        #endregion Constructors
    }

    /// <summary>
    /// Especificación para combinar dos especificaciones con un operador OR.
    /// </summary>
    internal class OrSpecification<T> : Specification<T>
        where T : class
    {
        #region Fields

        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        #endregion Fields

        #region Constructors

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _left = left ?? throw new ArgumentNullException(nameof(left));

            var paramExpr = Expression.Parameter(typeof(T));
            var exprBody = Expression.OrElse(
                Expression.Invoke(left.Criteria, paramExpr),
                Expression.Invoke(right.Criteria, paramExpr));
            Criteria = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

            Includes.AddRange(left.Includes.Union(right.Includes));
            IncludeStrings.AddRange(left.IncludeStrings.Union(right.IncludeStrings));
            if (left.OrderBy != null)
                ApplyOrderBy(left.OrderBy);
            else if (left.OrderByDescending != null)
                ApplyOrderByDescending(left.OrderByDescending);
            else if (right.OrderBy != null)
                ApplyOrderBy(right.OrderBy);
            else if (right.OrderByDescending != null)
                ApplyOrderByDescending(right.OrderByDescending);
        }

        #endregion Constructors
    }
}