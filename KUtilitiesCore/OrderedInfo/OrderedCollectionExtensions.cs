using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace KUtilitiesCore.OrderedInfo
{
    internal static class OrderedCollectionExtensions
    {
        #region Methods

        /// <summary>
        /// Obtiene las propiedades de un tipo con sus nombres y nombres(display) para mostrar
        /// </summary>
        /// <typeparam name="T">Tipo del que se recuperarán las propiedades</typeparam>
        /// <param name="onlySupportedTypes">Indica si solo se considerarán tipos soportados</param>
        /// <returns>Una colección de <see cref="PNameInfo"/> con las propiedades</returns>
        public static IEnumerable<PNameInfo> GetPropertyNames<T>(bool onlySupportedTypes = true)
            where T : class
        {
            var properties = typeof(T).GetProperties();
            var supportedTypes = new[] { typeof(int), typeof(string), typeof(decimal), typeof(double),
                                     typeof(DateTime), typeof(bool), typeof(Guid) };

            return properties
                .Where(p => p.GetMethod?.IsPublic == true)
                .Where(p => !onlySupportedTypes || supportedTypes.Contains(p.PropertyType))
                .Select(p =>
                {
                    var displayName = p.GetCustomAttributes(typeof(DisplayAttribute), false)
                        .Cast<DisplayAttribute>()
                        .FirstOrDefault()?.Name
                        ?? p.Name;

                    return new PNameInfo(p.Name, displayName);
                });
        }
        /// <summary>
        /// Ordena de manera ascendente los elementos de una secuencia en funcion del nombre de una propiedad.
        /// </summary>
        public static IQueryable<T> OrderBy<T>(
            this IQueryable<T> source,
            string property)
        {
            return ApplyOrder<T>(source, property, "OrderBy");
        }

        /// <summary>
        /// Ordena de manera Decendente los elementos de una secuencia en funcion del nombre de una propiedad.
        /// </summary>
        public static IQueryable<T> OrderByDescending<T>(
            this IQueryable<T> source,
            string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }

        /// <summary>
        /// Ordena de manera ascendente los elementos de una secuencia en funcion del nombre de una propiedad.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IQueryable<T> ThenBy<T>(
            this IQueryable<T> source,
            string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }

        /// <summary>
        /// Ordena de manera desendente los elementos de una secuencia en funcion del nombre de una propiedad.
        /// </summary>
        public static IQueryable<T> ThenByDescending<T>(
            this IQueryable<T> source,
            string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }

        private static IQueryable<T> ApplyOrder<T>(
             IQueryable<T> source,
             string property,
             string methodName)
        {
            PropertyInfo pi = null;
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            string[] props = property.Split('.');

            foreach (string prop in props)
            {
                pi = type.GetProperty(prop);
                if (pi == null) throw new Exception($"No se encuentra la propiedad: {prop}");
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);
            object result = typeof(Queryable).GetMethods().Single(
            method => method.Name == methodName
                    && method.IsGenericMethodDefinition
                    && method.GetGenericArguments().Length == 2
                    && method.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), type)
            .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
        #endregion Methods
    }
}