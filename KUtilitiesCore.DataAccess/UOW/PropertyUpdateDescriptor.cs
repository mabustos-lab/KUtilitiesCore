using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.UOW
{
    /// <summary>
    /// Describe una actualización de propiedad para operaciones masivas.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de la entidad a actualizar.</typeparam>
    public class PropertyUpdateDescriptor<TEntity> where TEntity : class
    {
        /// <summary>
        /// Selector de la propiedad a actualizar (ej. p => p.Name).
        /// Debe ser una expresión que seleccione un miembro.
        /// </summary>
        public LambdaExpression PropertySelector { get; }

        /// <summary>
        /// Expresión que define el nuevo valor para la propiedad.
        /// Puede ser una constante o una expresión basada en la entidad (ej. p => "Nuevo Valor" o p => p.Price * 1.1M).
        /// </summary>
        public LambdaExpression ValueExpression { get; }

        /// <summary>
        /// Constructor interno para controlar la creación. Usar los métodos factory Create.
        /// </summary>
        /// <param name="propertySelector">Selector de la propiedad.</param>
        /// <param name="valueExpression">Expresión del nuevo valor.</param>
        internal PropertyUpdateDescriptor(LambdaExpression propertySelector, LambdaExpression valueExpression)
        {
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));
            if (valueExpression == null)
                throw new ArgumentNullException(nameof(valueExpression));
            // Validación: debe ser una expresión lambda de acceso directo a propiedad (p => p.Prop)
            // o una conversión explícita (p => (object)p.Prop)
            bool isValid = ValidatePropertySelector(propertySelector.Body);
            if (!isValid)
            {
                throw new ArgumentException(
                    "El selector de propiedad debe ser una expresión de acceso directo a una propiedad de la entidad (ej. p => p.PropertyName). " +
                    "No se permiten expresiones complejas, métodos, campos o propiedades anidadas.",
                    nameof(propertySelector));
            }

            PropertySelector = propertySelector;
            ValueExpression = valueExpression;
        }

        private static bool ValidatePropertySelector(Expression selectorBody)
        {
            return selectorBody switch
            {
                // Caso directo: p => p.Prop
                MemberExpression member => IsValidPropertyAccess(member),
                // Caso conversión: p => (object)p.Prop
                UnaryExpression unary when unary.NodeType == ExpressionType.Convert =>
                    IsValidPropertyAccess(unary.Operand as MemberExpression),
                _ => false
            };           
        }

        private static bool IsValidPropertyAccess(MemberExpression memberExpr)
        {
            return memberExpr?.Expression is ParameterExpression param
                   && param.Type == typeof(TEntity)
                   && memberExpr.Member.MemberType == MemberTypes.Property;
        }

        /// <summary>
        /// Crea un descriptor de actualización de propiedad donde el nuevo valor es una expresión.
        /// </summary>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="propertySelector">Expresión para seleccionar la propiedad (ej. entity => entity.PropertyName).</param>
        /// <param name="valueExpression">Expresión para calcular el nuevo valor (ej. entity => entity.OldValue + 10).</param>
        /// <returns>Un nuevo PropertyUpdateDescriptor.</returns>
        public static PropertyUpdateDescriptor<TEntity> Create<TProperty>(
            Expression<Func<TEntity, TProperty>> propertySelector,
            Expression<Func<TEntity, TProperty>> valueExpression)
        {
            return new PropertyUpdateDescriptor<TEntity>(propertySelector, valueExpression);
        }

        /// <summary>
        /// Crea un descriptor de actualización de propiedad donde el nuevo valor es una constante.
        /// </summary>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="propertySelector">Expresión para seleccionar la propiedad (ej. entity => entity.PropertyName).</param>
        /// <param name="constantValue">El valor constante para asignar a la propiedad.</param>
        /// <returns>Un nuevo PropertyUpdateDescriptor.</returns>
        public static PropertyUpdateDescriptor<TEntity> Create<TProperty>(
            Expression<Func<TEntity, TProperty>> propertySelector,
            TProperty constantValue)
        {
            // Envuelve el valor constante en una expresión lambda que toma TEntity como parámetro
            // para que coincida con la firma esperada por ValueExpression.
            // El parámetro de la entidad no se usa en el cuerpo de esta lambda específica.
            ParameterExpression entityParameter = Expression.Parameter(typeof(TEntity), "e_const_val_param");
            ConstantExpression constantExpr = Expression.Constant(constantValue, typeof(TProperty));
            Expression<Func<TEntity, TProperty>> valueLambda = Expression.Lambda<Func<TEntity, TProperty>>(constantExpr, entityParameter);

            return new PropertyUpdateDescriptor<TEntity>(propertySelector, valueLambda);
        }
    }
}
