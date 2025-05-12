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
    /// Clase base para implementaciones de ISpecification.
    /// Proporciona una estructura común para definir especificaciones.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    public abstract class Specification<T> : ISpecification<T>
    {
        /// <inheritdoc/>
        public virtual Expression<Func<T, bool>> Criteria { get; protected set; } // Permitir set en clases derivadas

        /// <inheritdoc/>
        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        /// <inheritdoc/>
        public List<string> IncludeStrings { get; } = new List<string>();

        /// <inheritdoc/>
        public Expression<Func<T, object>> OrderBy { get; private set; }

        /// <inheritdoc/>
        public Expression<Func<T, object>> OrderByDescending { get; private set; }

        /// <inheritdoc/>
        public int Take { get; private set; }

        /// <inheritdoc/>
        public int Skip { get; private set; }

        /// <inheritdoc/>
        public bool IsPagingEnabled { get; private set; } = false;

        public IPagingOptions PagingOptions { get; private set; }

        /// <summary>
        /// Constructor base para especificaciones.
        /// </summary>
        /// <param name="criteria">La expresión de criterio inicial (opcional).</param>
        protected Specification(Expression<Func<T, bool>> criteria = null)
        {
            Criteria = criteria;
            PagingOptions=new PagingOptions();
        }

        /// <summary>
        /// Añade una expresión de inclusión de propiedad de navegación.
        /// </summary>
        /// <param name="includeExpression">La expresión de inclusión.</param>
        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Añade una cadena de inclusión de propiedad de navegación.
        /// </summary>
        /// <param name="includeString">La cadena de inclusión (ej. "Orders.OrderItems").</param>
        protected virtual void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

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

        /// <summary>
        /// Aplica la paginación.
        /// </summary>
        /// <param name="skip">Número de elementos a omitir.</param>
        /// <param name="take">Número de elementos a tomar.</param>
        protected virtual void ApplyPaging(int skip, int take)
        {
            // Validaciones básicas
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), "Skip no puede ser negativo.");
            if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take), "Take debe ser positivo para paginación.");

            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        /// <summary>
        /// Combina esta especificación con otra usando un operador AND lógico.
        /// </summary>
        /// <param name="other">La otra especificación a combinar.</param>
        /// <returns>Una nueva especificación que representa la combinación AND.</returns>
        public ISpecification<T> And(ISpecification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }

        /// <summary>
        /// Combina esta especificación con otra usando un operador OR lógico.
        /// </summary>
        /// <param name="other">La otra especificación a combinar.</param>
        /// <returns>Una nueva especificación que representa la combinación OR.</returns>
        public ISpecification<T> Or(ISpecification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }

        /// <summary>
        /// Niega esta especificación.
        /// </summary>
        /// <returns>Una nueva especificación que representa la negación de la actual.</returns>
        public ISpecification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }
    /// <summary>
    /// Especificación para combinar dos especificaciones con un operador AND.
    /// </summary>
    internal class AndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _left = left ?? throw new ArgumentNullException(nameof(left));

            // Combinar criterios
            var paramExpr = Expression.Parameter(typeof(T));
            var exprBody = Expression.AndAlso(Expression.Invoke(left.Criteria, paramExpr), Expression.Invoke(right.Criteria, paramExpr));
            Criteria = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

            // Combinar Includes (evitando duplicados)
            Includes.AddRange(left.Includes.Union(right.Includes));
            IncludeStrings.AddRange(left.IncludeStrings.Union(right.IncludeStrings));

            // La ordenación y paginación generalmente se toman de una de las especificaciones
            // o se aplican externamente. Aquí, por simplicidad, no se combinan automáticamente.
            // Se podría dar prioridad a 'left' o 'right', o requerir que solo una tenga estas propiedades.
        }
    }

    /// <summary>
    /// Especificación para combinar dos especificaciones con un operador OR.
    /// </summary>
    internal class OrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _left = left ?? throw new ArgumentNullException(nameof(left));

            var paramExpr = Expression.Parameter(typeof(T));
            var exprBody = Expression.OrElse(Expression.Invoke(left.Criteria, paramExpr), Expression.Invoke(right.Criteria, paramExpr));
            Criteria = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

            Includes.AddRange(left.Includes.Union(right.Includes));
            IncludeStrings.AddRange(left.IncludeStrings.Union(right.IncludeStrings));
        }
    }

    /// <summary>
    /// Especificación para negar otra especificación.
    /// </summary>
    internal class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _original;

        public NotSpecification(ISpecification<T> original)
        {
            _original = original ?? throw new ArgumentNullException(nameof(original));

            var paramExpr = Expression.Parameter(typeof(T));
            var exprBody = Expression.Not(Expression.Invoke(original.Criteria, paramExpr));
            Criteria = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

            Includes.AddRange(original.Includes);
            IncludeStrings.AddRange(original.IncludeStrings);
            // La ordenación y paginación se heredan o se ignoran según el diseño.
        }
    }
}
