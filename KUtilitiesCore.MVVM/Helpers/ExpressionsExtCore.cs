using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Helpers
{
    internal static class ExpressionsExt
    {
        ///// <summary>
        ///// Obtiene la ruta completa a una propiedad a través de una expresión.
        ///// </summary>
        ///// <typeparam name="TObject">Tipo del objeto del cual se obtendrá la propiedad.</typeparam>
        ///// <typeparam name="TProperty">Tipo de la propiedad a la que se accederá.</typeparam>
        ///// <param name="expression">Expresión lambda que define la ruta a la propiedad.</param>
        ///// <returns>La ruta completa como una cadena separada por puntos.</returns>
        //public static string GetFullPathProperty<TObject, TProperty>(this Expression<Func<TObject, TProperty>> expression)
        //{
        //    MemberExpression currentMember = null;

        //    if (!TryFindMemberExpression(expression.Body, ref currentMember))
        //        return string.Empty;

        //    List<string> memberNames = new List<string>();

        //    do
        //    {
        //        memberNames.Add(currentMember.Member.Name);
        //    }
        //    while (TryFindMemberExpression(currentMember.Expression, ref currentMember));

        //    memberNames.Reverse();
        //    return string.Join(".", memberNames);
        //}

        ///// <summary>
        ///// Intenta encontrar una expresión de miembro en una expresión dada.
        ///// </summary>
        ///// <param name="expression">Expresión a analizar.</param>
        ///// <param name="memberExpression">Referencia para almacenar la expresión de miembro encontrada.</param>
        ///// <returns>true si se encontró una expresión de miembro, false en caso contrario.</returns>
        //internal static bool TryFindMemberExpression(Expression expression, ref MemberExpression memberExpression)
        //{
        //    memberExpression = expression as MemberExpression;
        //    if (memberExpression != null)
        //        return true;

        //    if (IsConversion(expression) && expression is UnaryExpression unaryExp)
        //    {
        //        memberExpression = unaryExp.Operand as MemberExpression;
        //        return memberExpression != null;
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// Verifica si una expresión es una conversión implícita.
        ///// </summary>
        ///// <param name="expression">Expresión a analizar.</param>
        ///// <returns>true si la expresión es una conversión, false en caso contrario.</returns>
        //internal static bool IsConversion(Expression expression)
        //    => expression.NodeType == ExpressionType.Convert ||
        //       expression.NodeType == ExpressionType.ConvertChecked;

        /// <summary>
        /// Valida si una expresión no es una expresión de parámetro.
        /// </summary>
        /// <param name="expression">Expresión a validar.</param>
        /// <exception cref="ArgumentException">Si la expresión es de tipo ParameterExpression.</exception>
        internal static void CheckParameterExpression(Expression expression)
        {
            if (expression is ParameterExpression)
                throw new ArgumentException(nameof(expression));
        }

        /// <summary>
        /// Obtiene la propiedad asociada a una expresión lambda.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto.</typeparam>
        /// <typeparam name="TResult">Tipo del resultado de la expresión.</typeparam>
        /// <param name="expression">Expresión lambda que define la propiedad.</param>
        /// <returns>La propiedad encontrada como PropertyInfo.</returns>
        /// <exception cref="ArgumentException">Si no se pudo解析ar una propiedad de la expresión.</exception>
        internal static PropertyInfo GetArgumentPropertyStrict<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            MemberExpression memberExpr = ExtractMemberExpression(expression.Body);

            if (memberExpr == null)
                throw new ArgumentException(nameof(expression));

            CheckParameterExpression(memberExpr.Expression);
            return (PropertyInfo)memberExpr.Member;
        }

        private static MemberExpression ExtractMemberExpression(Expression expression)
        {
            if (expression is MemberExpression memberExp)
                return memberExp;

            if (expression is UnaryExpression unaryExp &&
                (unaryExp.NodeType == ExpressionType.Convert ||
                 unaryExp.NodeType == ExpressionType.ConvertChecked))
            {
                return unaryExp.Operand as MemberExpression;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el método getter para una propiedad de una interfaz.
        /// </summary>
        /// <typeparam name="TInterface">Tipo de la interfaz.</typeparam>
        /// <param name="instance">Instancia de la interfaz.</param>
        /// <param name="methodName">Nombre del método getter (formato get_PropertyName).</param>
        /// <returns>MethodInfo del método getter.</returns>
        internal static MethodInfo GetGetterMethod<TInterface>(TInterface instance, string methodName)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            InterfaceMapping interfaceMap = instance.GetType().GetInterfaceMap(typeof(TInterface));

            int methodIndex = interfaceMap.InterfaceMethods
                .Select((method, index) => new { method.Name, index })
                .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal))
                .Select(m => m.index)
                .FirstOrDefault();

            if (methodIndex == -1)
                throw new ArgumentException(nameof(methodName));

            return interfaceMap.TargetMethods[methodIndex];
        }

        /// <summary>
        /// Verifica si una propiedad tiene una implementación implícita en una interfaz.
        /// </summary>
        /// <typeparam name="TInterface">Tipo de la interfaz.</typeparam>
        /// <typeparam name="TPropertyType">Tipo de la propiedad.</typeparam>
        /// <param name="instance">Instancia de la interfaz.</param>
        /// <param name="propertyExpression">Expresión lambda que define la propiedad.</param>
        /// <param name="tryInvoke">Indica si se intentará invocar el método getter para verificar su funcionalidad.</param>
        /// <returns>true si la propiedad tiene una implementación válida, false en caso contrario.</returns>
        internal static bool PropertyHasImplicitImplementation<TInterface, TPropertyType>(TInterface instance,
            Expression<Func<TInterface, TPropertyType>> propertyExpression,
            bool tryInvoke = true)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            string propertyName = GetArgumentPropertyStrict(propertyExpression).Name;
            string getterName = $"get_{propertyName}";

            try
            {
                MethodInfo getterMethod = GetGetterMethod(instance, getterName);

                if (!getterMethod.IsPublic || !string.Equals(getterMethod.Name, getterName, StringComparison.Ordinal))
                    return false;

                if (!tryInvoke)
                    return true;

                getterMethod.Invoke(instance, null);
                return true;
            }
            catch (TargetException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (TargetParameterCountException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (MethodAccessException)
            {
                return false;
            }
        }
    }
}
