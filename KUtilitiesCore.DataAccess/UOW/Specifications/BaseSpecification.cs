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
    /// <summary>
    /// Base no genérica para especificaciones: aporta los parámetros SQL con nombre
    /// que las implementaciones de repositorio pueden usar al materializar la consulta.
    /// </summary>
    public abstract class BaseSpecification : PagingOptions, ISpecificationBase
    {
        /// <inheritdoc/>
        public virtual IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Base para construir especificaciones tipadas sobre una entidad: criterio,
    /// includes, ordenación y parámetros. Las clases derivadas exponen métodos
    /// con intención (p. ej. <c>AplicarFiltroActivo()</c>) en lugar de exponer
    /// estos miembros protegidos directamente.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad al que aplica la especificación.</typeparam>
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

        /// <summary>
        /// Inicializa la especificación con el criterio que deben cumplir las entidades devueltas.
        /// </summary>
        /// <param name="criteria">Expresión que define el criterio de la consulta.</param>
        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }
        /// <summary>
        /// Registra un parámetro SQL con nombre para que la implementación del repositorio
        /// lo utilice al materializar la consulta.
        /// </summary>
        /// <param name="name">Nombre del parámetro.</param>
        /// <param name="value">Valor del parámetro.</param>
        protected virtual void AddParameter(string name, object value)
            => Parameters[name] = value;
        /// <summary>
        /// Añade una ruta de navegación (include) en formato cadena, útil para includes
        /// que no pueden expresarse como expresión tipada.
        /// </summary>
        /// <param name="includeString">Ruta de navegación (p. ej. <c>"Cliente.Pedidos"</c>).</param>
        protected virtual void AddInclude(string includeString)
            => IncludeStrings.Add(includeString);
        /// <summary>
        /// Establece la expresión de ordenación ascendente de la consulta.
        /// </summary>
        /// <param name="orderByExpression">Expresión de ordenación.</param>
        protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
         => OrderBy = orderByExpression;
        /// <summary>
        /// Establece la expresión de ordenación descendente de la consulta.
        /// </summary>
        /// <param name="orderByDescendingExpression">Expresión de ordenación descendente.</param>
        protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
            => OrderByDescending = orderByDescendingExpression;
        /// <summary>
        /// Marca la consulta para que las entidades se materialicen sin seguimiento de cambios,
        /// apropiado en escenarios de solo lectura.
        /// </summary>
        protected virtual void ApplyNoTracking()
            => IsAsNoTracking = true;
    }
    /// <summary>
    /// Especificación para buscar una entidad por una condición de identidad.
    /// </summary>
    public class EntityByIdSpecification<T> : BaseSpecification<T>, ISpecification<T> where T : class
    {
        /// <summary>
        /// Construye la especificación a partir del predicado de identidad indicado.
        /// </summary>
        /// <param name="idPredicate">Predicado que identifica la entidad buscada.</param>
        public EntityByIdSpecification(Expression<Func<T, bool>> idPredicate)
            : base(idPredicate)
        {
        }
    }
}
