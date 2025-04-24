using System;
using System.Linq;
using System.Linq.Expressions;

namespace KUtilitiesCore.Data.Validation.Helper
{
    // ----------------------------------------------------------------------
    // Helper para extraer nombres de propiedades (usado internamente)
    // ----------------------------------------------------------------------
    internal static class PropertyRuleExtensions
    {
        public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                return member.Member.Name;
            }
            throw new ArgumentException("Expression is not a property access", nameof(expression));
        }
    }
}
