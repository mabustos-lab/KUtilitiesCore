using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace KUtilitiesCore.OrderedInfo
{
    internal static class OrderedCollectionExtensions
    {
        private static readonly Type[] SupportedTypes =
        {
            typeof(int),
            typeof(string),
            typeof(decimal),
            typeof(double),
            typeof(DateTime),
            typeof(bool),
            typeof(Guid)
        };

        /// <summary>
        /// Obtiene las propiedades de un tipo con sus nombres y nombres(display) para mostrar
        /// </summary>
        /// <typeparam name="T">Tipo del que se recuperarán las propiedades</typeparam>
        /// <param name="onlySupportedTypes">Indica si solo se considerarán tipos soportados</param>
        /// <returns>Una colección de <see cref="PNameInfo"/> con las propiedades</returns>
        public static IEnumerable<PropertyNameInfo> GetPropertyNames<T>(bool onlySupportedTypes = true)
            where T : class
        {
            return typeof(T)
              .GetProperties()
                .Where(p => IsPublicReadable(p) && IsSupportedType(p, onlySupportedTypes))
                .Select(ToPropertyNameInfo);
        }
        /// <summary>
        /// Ordena de manera ascendente los elementos de una secuencia en funcion del nombre de una propiedad.
        /// </summary>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        { return ApplyOrder(source, propertyName, "OrderBy"); }
        /// <summary>
        /// Ordena de manera Decendente los elementos de una secuencia en funcion del nombre de una propiedad.
        /// </summary>
        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        { return ApplyOrder<T>(source, property, "OrderByDescending"); }

        /// <summary>
        /// Ordena de manera ascendente los elementos de una secuencia en funcion del nombre de una propiedad.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IQueryable<T> ThenBy<T>(this IQueryable<T> source, string property)
        { return ApplyOrder<T>(source, property, "ThenBy"); }

        /// <summary>
        /// Ordena de manera desendente los elementos de una secuencia en funcion del nombre de una propiedad.
        /// </summary>
        public static IQueryable<T> ThenByDescending<T>(this IQueryable<T> source, string property)
        { return ApplyOrder<T>(source, property, "ThenByDescending"); }

        private static IQueryable<T> ApplyOrder<T>(IQueryable<T> source, string propertyName, string methodName)
        {
            var (lambdaExpression, propertyType) = CreatePropertyExpression<T>(propertyName);

            var method = QueryableMethodsCache.GetOrAddMethod<T>(methodName, propertyType);
            IOrderedQueryable<T>? result = method.Invoke(null, new object[] { source, lambdaExpression }) as IOrderedQueryable<T>;
            if (result is null)
                throw new InvalidOperationException("No se pudo aplicar el método de ordenación. El resultado fue nulo.");
            return result;
        }

        private static (LambdaExpression, Type) CreatePropertyExpression<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = ResolvePropertyPath(typeof(T), propertyName, parameter);
            return (Expression.Lambda(property.expression, parameter), property.propertyType);
        }

        private static (Expression expression, Type propertyType) ResolvePropertyPath(
            Type rootType,
            string propertyPath,
            ParameterExpression parameter)
        {
            Expression currentExpression = parameter;
            Type currentType = rootType;

            foreach (var prop in propertyPath.Split('.'))
            {
                var propertyInfo = currentType.GetProperty(prop) ??
                    throw new InvalidOperationException($"Propiedad no encontrada: {prop}");
                currentExpression = Expression.Property(currentExpression, propertyInfo);
                currentType = propertyInfo.PropertyType;
            }

            return (currentExpression, currentType);
        }

        private static bool IsPublicReadable(PropertyInfo property) => property.GetMethod?.IsPublic == true;

        private static bool IsSupportedType(PropertyInfo property, bool filterEnabled) => !filterEnabled ||
            SupportedTypes.Contains(property.PropertyType);

        private static PropertyNameInfo ToPropertyNameInfo(PropertyInfo property)
        {
            var displayName = property.GetCustomAttributes<DisplayAttribute>().FirstOrDefault()?.Name ?? property.Name;

            return new PropertyNameInfo(property.Name, displayName);
        }

        private static class QueryableMethodsCache
        {
            private static readonly ConcurrentDictionary<string, MethodInfo> _methods = new();

            public static MethodInfo GetOrAddMethod<T>(string methodName, Type propertyType)
            {
                var key = $"{typeof(T).Name}_{methodName}_{propertyType.Name}";

                return _methods.GetOrAdd(key, _ =>
                {
                    return typeof(Queryable).GetMethods()
                        .Single(m => m.Name == methodName
                                  && m.IsGenericMethodDefinition
                                  && m.GetGenericArguments().Length == 2
                                  && m.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(T), propertyType);
                });
            }
        }

        
    }
}