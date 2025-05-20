using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Helpers
{
    static class ExpressionsHelpers
    {
        #region Methods

        /// <summary>
        /// Valida si <see cref="Expression"/> no es del tipo <see cref="ParameterExpression"/>
        /// </summary>
        /// <param name="expression">Expresión a validar</param>
        public static void CheckParameterExpression(Expression expression)
        {
            if ( expression is not ParameterExpression)
                throw new ArgumentException(nameof(expression));
        }

        /// <summary>
        /// Genera una expresión que invoca una acción para establecer un valor a una propiedad de manera dinámica dada una expresión lambda
        /// </summary>
        /// <typeparam name="TTarget">Tipo del objeto que contiene la propiedad</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad</typeparam>
        /// <param name="selector">Expresión lambda que selecciona la propiedad</param>
        /// <returns>Expresión que invoca una acción para establecer el valor de la propiedad</returns>
        public static Expression<Action<TTarget, TProperty>> CreateSetter<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> selector)
        {
            var valueParam = Expression.Parameter(typeof(TProperty));
            var body = Expression.Assign(selector.Body, valueParam);
            return Expression.Lambda<Action<TTarget, TProperty>>(body, selector.Parameters.Single(), valueParam);
        }

        /// <summary>
        /// Obtiene la información del método de una expresión lambda estricta
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <typeparam name="TResult">Tipo del resultado</typeparam>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Información del método</returns>
        public static MethodInfo GetArgumentFunctionStrict<T, TResult>(Expression<Func<T, TResult>> expression)
            => GetArgumentMethodStrictCore(expression);

        /// <summary>
        /// Obtiene la información del método de una expresión lambda estricta
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Información del método</returns>
        public static MethodInfo GetArgumentMethodStrict<T>(Expression<Action<T>> expression)
            => GetArgumentMethodStrictCore(expression);

        /// <summary>
        /// Obtiene la información del método de una expresión lambda estricta
        /// </summary>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Información del método</returns>
        public static MethodInfo GetArgumentMethodStrictCore(LambdaExpression expression)
        {
            var methodCallExpression = GetMethodCallExpression(expression);
            CheckParameterExpression(methodCallExpression.Object!);
            return methodCallExpression.Method;
        }

        /// <summary>
        /// Obtiene la información de la propiedad de una expresión lambda estricta
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <typeparam name="TResult">Tipo del resultado</typeparam>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Información de la propiedad</returns>
        public static PropertyInfo GetArgumentPropertyStrict<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            MemberExpression body = expression.Body switch
            {
                MemberExpression memberExpression => memberExpression,
                UnaryExpression unaryExpression when unaryExpression.NodeType == ExpressionType.Convert => (MemberExpression)unaryExpression.Operand,
                _ => throw new ArgumentException(nameof(expression))
            };
            CheckParameterExpression(body.Expression!);
            return (PropertyInfo)body.Member;
        }

        /// <summary>
        /// Obtiene la información del constructor de una expresión lambda
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <param name="commandMethodExpression">Expresión lambda</param>
        /// <returns>Información del constructor</returns>
        public static ConstructorInfo GetConstructor<T>(Expression<Func<T>> commandMethodExpression)
            => GetConstructorCore(commandMethodExpression);

        /// <summary>
        /// Obtiene la información del constructor de una expresión lambda
        /// </summary>
        /// <param name="commandMethodExpression">Expresión lambda</param>
        /// <returns>Información del constructor</returns>
        public static ConstructorInfo GetConstructorCore(LambdaExpression commandMethodExpression)
        {
            if (commandMethodExpression.Body is not NewExpression body)
                throw new ArgumentException(nameof(commandMethodExpression));
            return body.Constructor!;
        }

        /// <summary>
        /// Obtiene el método getter de una interfaz
        /// </summary>
        /// <typeparam name="TInterface">Tipo de la interfaz</typeparam>
        /// <param name="iface">Instancia de la interfaz</param>
        /// <param name="getMethodName">Nombre del método getter</param>
        /// <returns>Información del método getter</returns>
        public static MethodInfo GetGetterMethod<TInterface>(TInterface iface, string getMethodName)
        {
            var interfaceMap = typeof(TInterface).GetInterfaceMap(typeof(TInterface));
            var index = interfaceMap.InterfaceMethods
                .Select((m, i) => new { m.Name, Index = i })
                .First(m => string.Equals(m.Name, getMethodName, StringComparison.Ordinal))
                .Index;
            return interfaceMap.TargetMethods[index];
        }

        /// <summary>
        /// Obtiene la expresión de miembro de una expresión lambda
        /// </summary>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Expresión de miembro</returns>
        public static MemberExpression GetMemberExpression(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var body = expression.Body is UnaryExpression unaryExpression ? unaryExpression.Operand : expression.Body;

            if (body is not MemberExpression memberExpression)
                throw new ArgumentException(nameof(expression));

            return memberExpression;
        }

        /// <summary>
        /// Obtiene la información del método de una expresión lambda
        /// </summary>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Información del método</returns>
        public static MethodInfo GetMethod(LambdaExpression expression)
            => GetMethodCallExpression(expression).Method;

        /// <summary>
        /// Obtiene la expresión de llamada a método de una expresión lambda
        /// </summary>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Expresión de llamada a método</returns>
        public static MethodCallExpression GetMethodCallExpression(LambdaExpression expression)
        {
            if (expression.Body is InvocationExpression invocationExpression)
                expression = (LambdaExpression)invocationExpression.Expression;
            return (MethodCallExpression)expression.Body;
        }

        /// <summary>
        /// Obtiene el nombre del método de una expresión lambda
        /// </summary>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Nombre del método</returns>
        public static string GetMethodName(Expression<Action> expression)
            => GetMethod(expression).Name;

        /// <summary>
        /// Obtiene la colección de propiedades de un tipo
        /// </summary>
        /// <param name="sourceType">Tipo del objeto</param>
        /// <returns>Colección de propiedades</returns>
        public static PropertyDescriptorCollection GetPropertiesCore(Type sourceType)
            => TypeDescriptor.GetProperties(sourceType);

        /// <summary>
        /// Obtiene la propiedad de una expresión lambda
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad</typeparam>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Descriptor de la propiedad</returns>
        public static PropertyDescriptor? GetProperty<T, TProperty>(Expression<Func<T, TProperty>> expression)
            => GetPropertiesCore(typeof(T))[GetPropertyName(expression)];

        /// <summary>
        /// Obtiene la información de la propiedad de una expresión
        /// </summary>
        /// <param name="propertyExpression">Expresión de la propiedad</param>
        /// <returns>Información de la propiedad</returns>
        public static MemberInfo GetPropertyInformation(Expression propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            MemberExpression? memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null && propertyExpression is UnaryExpression unaryExpr &&
                    (unaryExpr.NodeType == ExpressionType.Convert || unaryExpr.NodeType == ExpressionType.ConvertChecked))
            {
                memberExpr = unaryExpr.Operand as MemberExpression;
            }

            if (memberExpr == null || memberExpr.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException("La expresión no representa una propiedad.", nameof(propertyExpression));

            return memberExpr.Member;
        }

        /// <summary>
        /// Obtiene el nombre de la propiedad de una expresión lambda
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad</typeparam>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Nombre de la propiedad</returns>
        public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
            => GetPropertyNameCore(expression);

        /// <summary>
        /// Obtiene el nombre de la propiedad de una expresión lambda
        /// </summary>
        /// <param name="expression">Expresión lambda</param>
        /// <returns>Nombre de la propiedad</returns>
        public static string GetPropertyNameCore(LambdaExpression expression)
        {
            var memberExpression = GetMemberExpression(expression);
            if (memberExpression.Expression is MemberExpression innerMemberExpression && IsPropertyExpression(innerMemberExpression))
                throw new ArgumentException(nameof(expression));
            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Indica si la expresión es una conversión
        /// </summary>
        /// <param name="exp">Expresión</param>
        /// <returns>Verdadero si es una conversión, falso en caso contrario</returns>
        public static bool IsConversion(Expression exp)
            => exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.ConvertChecked;

        /// <summary>
        /// Indica si la expresión da acceso a una propiedad
        /// </summary>
        /// <param name="expression">Expresión de miembro</param>
        /// <returns>Verdadero si la expresión da acceso a una propiedad, falso en caso contrario</returns>
        public static bool IsPropertyExpression(MemberExpression expression)
            => expression != null && expression.Member.MemberType == MemberTypes.Property;

        /// <summary>
        /// Indica si una propiedad tiene una implementación implícita
        /// </summary>
        /// <typeparam name="TInterface">Tipo de la interfaz</typeparam>
        /// <typeparam name="TPropertyType">Tipo de la propiedad</typeparam>
        /// <param name="iface">Instancia de la interfaz</param>
        /// <param name="property">Expresión lambda de la propiedad</param>
        /// <param name="tryInvoke">Indica si se debe intentar invocar el método getter</param>
        /// <returns>Verdadero si la propiedad tiene una implementación implícita, falso en caso contrario</returns>
        public static bool PropertyHasImplicitImplementation<TInterface, TPropertyType>(TInterface iface, 
            Expression<Func<TInterface, TPropertyType>> property, bool tryInvoke = true)            
        {
            if (iface is null)
                throw new ArgumentNullException(nameof(iface));

            var name = GetArgumentPropertyStrict(property).Name;
            var str = $"get_{name}";
            var getMethod = GetGetterMethod(iface, str);

            if (!getMethod.IsPublic || !string.Equals(getMethod.Name, str))
                return false;

            try
            {
                if (tryInvoke)
                    getMethod.Invoke(iface, null);
                return true;
            }
            catch (Exception ex) when (ex is TargetException || ex is ArgumentException || ex is TargetParameterCountException || ex is MethodAccessException || ex is InvalidOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// Intenta encontrar una expresión de miembro en una expresión
        /// </summary>
        /// <param name="exp">Expresión</param>
        /// <param name="memberExp">Expresión de miembro</param>
        /// <returns>Verdadero si se encuentra una expresión de miembro, falso en caso contrario</returns>
        public static bool TryFindMemberExpression(Expression? exp, ref MemberExpression? memberExp)
        {
            if(exp is null)
                return false;
            memberExp = exp as MemberExpression;
            if (memberExp is not null)
                return true;

            if (IsConversion(exp) && exp is UnaryExpression unaryExp)
            {
                memberExp = unaryExp.Operand as MemberExpression;
                if (memberExp is not null)
                    return true;
            }

            return false;
        }

        #endregion Methods
    }
}
