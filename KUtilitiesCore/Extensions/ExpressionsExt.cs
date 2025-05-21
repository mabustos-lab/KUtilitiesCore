using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class ExpressionsExt
    {
        #region Methods

        /// <summary>
        /// Obtiene la propiedad de una expresión lambda.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto fuente.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="propertyLambda">Expresión lambda que representa la propiedad.</param>
        /// <returns>Información de la propiedad.</returns>
        /// <exception cref="ArgumentException">Si la expresión no es una propiedad válida.</exception>
        public static PropertyInfo GetPropertyFromLambda<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
            where TSource : class
        {
            Type type = typeof(TSource);
            if (propertyLambda.Body is not MemberExpression member)
                throw new ArgumentException($"La expresión '{propertyLambda}' hace referencia a un método, no a una propiedad.");

            if (member.Member is not PropertyInfo propInfo)
                throw new ArgumentException($"La expresión '{propertyLambda}' hace referencia a un campo, no a una propiedad.");

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType!))
                throw new ArgumentException($"La expresión '{propertyLambda}' hace referencia a una propiedad que no pertenece al tipo {type}.");

            return propInfo;
        }

        /// <summary>
        /// Obtiene la ruta completa a una propiedad a través de una expresión.
        /// </summary>
        /// <typeparam name="TObject">Tipo del objeto.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="expression">Expresión que genera la ruta de la propiedad.</param>
        /// <returns>Ruta completa de la propiedad.</returns>
        public static string GetFullPathProperty<TObject, TProperty>(this Expression<Func<TObject, TProperty>> expression)
        {
            MemberExpression? memberExp = null;
            if (!Helpers.ExpressionsHelpers.TryFindMemberExpression(expression.Body, ref memberExp))
                return string.Empty;

            var memberNames = new Stack<string>();
            do
            {
                if (memberExp?.Member != null)
                    memberNames.Push(memberExp.Member.Name);
            } while (Helpers.ExpressionsHelpers.TryFindMemberExpression(memberExp?.Expression, ref memberExp));

            return string.Join(".", memberNames);
        }

        /// <summary>
        /// Obtiene el valor de una propiedad por medio de una ruta representada por una cadena de texto.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto fuente.</typeparam>
        /// <typeparam name="TResult">Tipo del resultado.</typeparam>
        /// <param name="Source">Objeto fuente.</param>
        /// <param name="pathProperty">Ruta de la propiedad.</param>
        /// <returns>Valor de la propiedad.</returns>
        public static TResult? GetPropertyValueOfPath<TSource, TResult>(TSource Source, string pathProperty)
            where TSource: class
        {
            return (TResult?)Source.GetPropertyValueOfPath(pathProperty);
        }

        /// <summary>
        /// Obtiene el valor de una propiedad por medio de una expresión lambda.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto fuente.</typeparam>
        /// <typeparam name="TResult">Tipo del resultado.</typeparam>
        /// <param name="Source">Objeto fuente.</param>
        /// <param name="expression">Expresión lambda que representa la propiedad.</param>
        /// <returns>Valor de la propiedad.</returns>
        public static TResult? GetPropertyValueOfPath<TSource, TResult>(this TSource Source, Expression<Func<TSource, TResult>> expression)
            where TSource : class
            => (TResult?)Source.GetPropertyValueOfPath(expression.GetFullPathProperty());

        /// <summary>
        /// Obtiene el valor de una propiedad de un objeto dada una cadena de texto.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto.</typeparam>
        /// <param name="source">Objeto fuente.</param>
        /// <param name="pathProperty">Ruta de la propiedad.</param>
        /// <returns>Valor de la propiedad.</returns>
        public static object? GetPropertyValueOfPath<TSource>(this TSource source, string pathProperty)
            where TSource :class
        {
            if (source  is null)
                throw new ArgumentNullException(nameof(source));
            object? ParentSource = source;
            string CorrectPropertyName = pathProperty;

            if (CorrectPropertyName.Contains('.'))
            {
                var Properties = pathProperty.Split('.');
                CorrectPropertyName = Properties[Properties.Length - 1];

                foreach (var sProperty in Properties.Take(Properties.Length - 1))
                {
                    var currentpi = ParentSource?.GetType().GetProperty(sProperty);
                    if (currentpi == null) return null;
                    ParentSource = currentpi.GetValue(ParentSource);
                }
            }

            var currentPI = ParentSource?.GetType().GetProperty(CorrectPropertyName);
            return currentPI?.GetValue(ParentSource);
        }

        /// <summary>
        /// Establece el valor en la propiedad designada por el objeto Expression.
        /// </summary>
        /// <typeparam name="TTarget">Tipo del objeto destino.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="Source">Objeto fuente.</param>
        /// <param name="expr">Expresión lambda que representa la propiedad.</param>
        /// <param name="Value">Valor a establecer.</param>
        /// <returns>Objeto fuente con el valor establecido.</returns>
        public static TTarget SetValue<TTarget, TProperty>(this TTarget Source, Expression<Func<TTarget, TProperty>> expr, TProperty Value)
        {
            Helpers.ExpressionsHelpers.CreateSetter(expr).Compile()(Source, Value);
            return Source;
        }

        /// <summary>
        /// Convierte una expresión de acceso a una propiedad de un objeto a un PropertyInfo.
        /// </summary>
        /// <typeparam name="TObject">Tipo del objeto.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="expression">Expresión que representa la propiedad.</param>
        /// <returns>Información de la propiedad.</returns>
        public static PropertyInfo? ToPropertyInfo<TObject, TProperty>(this Expression<Func<TObject, TProperty>> expression)
        {
            MemberExpression? memberExp = null;
            if (Helpers.ExpressionsHelpers.TryFindMemberExpression(expression.Body, ref memberExp))
            {
                MemberExpression? lastMember;
                do
                {
                    lastMember = memberExp;
                } while (Helpers.ExpressionsHelpers.TryFindMemberExpression(memberExp?.Expression, ref memberExp));

                return lastMember?.Member as PropertyInfo;
            }

            return null;
        }

        #endregion Methods
    }
}
