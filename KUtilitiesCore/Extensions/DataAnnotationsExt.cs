using KUtilitiesCore.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class DataAnnotationsExt
    {
        #region Methods
        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario.
        /// </summary>
        /// <remarks>
        /// Este método y sus sobrecargas permiten obtener la descripción de una propiedad de diversas maneras:
        /// - Especificando el tipo fuente y el nombre de la propiedad.
        /// - Usando una instancia del objeto y el nombre de la propiedad.
        /// - Directamente desde un objeto <see cref="PropertyInfo"/>.
        /// - Mediante una expresión lambda que accede a la propiedad.
        /// La descripción se obtiene prioritariamente del atributo <see cref="DisplayAttribute.Description"/>,
        /// y si no está presente, del atributo <see cref="DescriptionAttribute"/>.
        /// </remarks>
        /// <typeparam name="TSource">El tipo de la clase que contiene la propiedad.</typeparam>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>La descripción de la propiedad; o una cadena vacía si no se encuentra.</returns>
        public static string DataAnnotationsDescription<TSource>(string propertyName)
        {
            return typeof(TSource).DataAnnotationsDescription(propertyName);
        }

        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario.
        /// </summary>
        /// <param name="Source">El tipo de la clase que contiene la propiedad.</param>
        /// <param name="propertyName">Nombre de la propiedad que se desea buscar.</param>
        /// <returns>La descripción de la propiedad; o una cadena vacía si no se encuentra.</returns>
        public static string DataAnnotationsDescription(this Type Source, string propertyName)
            => DataAnnotationsHelpers.GetDescriptionCore(Source, propertyName);

        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario.
        /// </summary>
        /// <typeparam name="TSource">El tipo del objeto que contiene la propiedad.</typeparam>
        /// <param name="source">La instancia del objeto.</param>
        /// <param name="propertyName">Nombre de la propiedad que se desea buscar.</param>
        /// <returns>La descripción de la propiedad; o una cadena vacía si no se encuentra.</returns>
        public static string DataAnnotationsDescription<TSource>(this TSource source, string propertyName)
        => DataAnnotationsHelpers.GetDescriptionCore(source?.GetType() ?? typeof(TSource), propertyName);

        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario.
        /// </summary>
        /// <param name="pi">El objeto <see cref="PropertyInfo"/> que representa la propiedad.</param>
        /// <returns>La descripción de la propiedad; o una cadena vacía si no se encuentra.</returns>
        public static string DataAnnotationsDescription(this PropertyInfo pi)
        {
            DisplayAttribute? ret = DataAnnotationsHelpers.GetAttribCore<DisplayAttribute>(pi);
            if (ret is null)
            {
                DescriptionAttribute? ret2 = DataAnnotationsHelpers.GetAttribCore<DescriptionAttribute>(pi);
                return ret2?.Description ?? string.Empty;
            }
            return ret?.GetDescription() ?? string.Empty;
        }

        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario.
        /// </summary>
        /// <typeparam name="TSource">El tipo de la clase que contiene la propiedad.</typeparam>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="propertyAccesor">Una expresión lambda que accede a la propiedad (ej. `x => x.PropertyName`).</param>
        /// <returns>La descripción de la propiedad; o una cadena vacía si no se encuentra.</returns>
        public static string DataAnnotationsDescription<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyAccesor)
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(propertyAccesor.Body);
            return DataAnnotationsHelpers.GetDescriptionCore(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Proporciona el texto de presentación de una propiedad, usualmente una cadena de formato.
        /// </summary>
        /// <remarks>
        /// Este método y sus sobrecargas permiten obtener la cadena de formato de una propiedad de diversas maneras:
        /// - Especificando el tipo fuente y el nombre de la propiedad.
        /// - Usando una instancia del objeto y el nombre de la propiedad.
        /// - Directamente desde un objeto <see cref="PropertyInfo"/>.
        /// - Mediante una expresión lambda que accede a la propiedad.
        /// La cadena de formato se obtiene del atributo <see cref="DisplayFormatAttribute.DataFormatString"/>.
        /// Si no se encuentra, se devuelve el nombre de la propiedad.
        /// </remarks>
        /// <param name="Source">El tipo de la clase que contiene la propiedad.</param>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>La cadena de formato de la propiedad; o el nombre de la propiedad si no se encuentra un formato.</returns>
        public static string DataAnnotationsDisplayFormat(this Type Source, string propertyName)
             => DataAnnotationsHelpers.GetDisplayFormatCore(Source, propertyName);

        /// <summary>
        /// Proporciona el texto de presentación de una propiedad, usualmente una cadena de formato.
        /// </summary>
        /// <typeparam name="TSource">El tipo del objeto que contiene la propiedad.</typeparam>
        /// <param name="source">La instancia del objeto.</param>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>La cadena de formato de la propiedad; o el nombre de la propiedad si no se encuentra un formato.</returns>
        public static string DataAnnotationsDisplayFormat<TSource>(this TSource source, string propertyName)
            => DataAnnotationsHelpers.GetDisplayFormatCore(source?.GetType() ?? typeof(TSource), propertyName);

        /// <summary>
        /// Proporciona el texto de presentación de una propiedad, usualmente una cadena de formato.
        /// </summary>
        /// <param name="pi">El objeto <see cref="PropertyInfo"/> que representa la propiedad.</param>
        /// <returns>La cadena de formato de la propiedad; o el nombre de la propiedad si no se encuentra un formato.</returns>
        public static string DataAnnotationsDisplayFormat(this PropertyInfo pi)
        {
            DisplayFormatAttribute? ret = DataAnnotationsHelpers.GetAttribCore<DisplayFormatAttribute>(pi);
            return ret?.DataFormatString ?? pi.Name;
        }

        /// <summary>
        /// Proporciona el texto de presentación de una propiedad, usualmente una cadena de formato.
        /// </summary>
        /// <typeparam name="TSource">El tipo de la clase que contiene la propiedad.</typeparam>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="propertyAccesor">Una expresión lambda que accede a la propiedad (ej. `x => x.PropertyName`).</param>
        /// <returns>La cadena de formato de la propiedad; o el nombre de la propiedad si no se encuentra un formato.</returns>
        public static string DataAnnotationsDisplayFormat<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyAccesor)
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(propertyAccesor.Body);
            return DataAnnotationsHelpers.GetDisplayFormatCore(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Proporciona el texto para mostrar de una propiedad, comúnmente usado para etiquetas en la UI.
        /// </summary>
        /// <remarks>
        /// Este método y sus sobrecargas permiten obtener el nombre para mostrar (Display Name) de una propiedad de varias maneras:
        /// - Usando una instancia del objeto y el nombre de la propiedad.
        /// - Especificando el tipo fuente y el nombre de la propiedad.
        /// - Directamente desde un objeto <see cref="PropertyInfo"/>.
        /// - Mediante una expresión lambda que accede a la propiedad.
        /// El nombre para mostrar se obtiene prioritariamente del atributo <see cref="DisplayAttribute.Name"/>,
        /// y si no está presente, del atributo <see cref="DisplayNameAttribute"/>. Si ninguno está presente,
        /// se devuelve el nombre de la propiedad.
        /// </remarks>
        /// <typeparam name="TSource">El tipo del objeto que contiene la propiedad.</typeparam>
        /// <param name="source">La instancia del objeto.</param>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>El nombre para mostrar de la propiedad; o el nombre de la propiedad si no se encuentra uno específico.</returns>
        public static string DataAnnotationsDisplayName<TSource>(this TSource source, string propertyName)
            => DataAnnotationsHelpers.GetDisplayNameCore(source?.GetType() ?? typeof(TSource), propertyName);

        /// <summary>
        /// Proporciona el texto para mostrar de una propiedad.
        /// </summary>
        /// <param name="SourceType">El tipo de la clase que contiene la propiedad.</param>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>Regresa el valor que contiene el atributo en DataAnnotations;
        /// de lo contrario, el nombre de la propiedad.</returns>
        public static string DataAnnotationsDisplayName(this Type SourceType, string propertyName)
            => DataAnnotationsHelpers.GetDisplayNameCore(SourceType, propertyName);

        /// <summary>
        /// Proporciona el texto para mostrar de una propiedad.
        /// </summary>
        /// <param name="pi">El objeto <see cref="PropertyInfo"/> que representa la propiedad.</param>
        /// <returns>El nombre para mostrar de la propiedad; o el nombre de la propiedad si no se encuentra uno específico.</returns>
        public static string DataAnnotationsDisplayName(this PropertyInfo pi)
        {
            DisplayAttribute? ret = DataAnnotationsHelpers.GetAttribCore<DisplayAttribute>(pi);
            if (ret is null)
            {
                DisplayNameAttribute? ret2 = DataAnnotationsHelpers.GetAttribCore<DisplayNameAttribute>(pi);
                return ret2?.DisplayName ?? pi.Name;
            }
            return ret?.GetName() ?? pi.Name;
        }

        /// <summary>
        /// Proporciona el texto para mostrar de una propiedad.
        /// </summary>
        /// <typeparam name="TSource">El tipo de la clase que contiene la propiedad.</typeparam>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="propertyAccesor">Una expresión lambda que accede a la propiedad (ej. `x => x.PropertyName`).</param>
        /// <returns>El nombre para mostrar de la propiedad; o el nombre de la propiedad si no se encuentra uno específico.</returns>
        public static string DataAnnotationsDisplayName<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyAccesor)
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(propertyAccesor.Body);
            return DataAnnotationsHelpers.GetDisplayNameCore(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Proporciona el nombre corto para mostrar de una propiedad.
        /// </summary>
        /// <remarks>
        /// Este método y sus sobrecargas permiten obtener el nombre corto para mostrar (Short Name) de una propiedad de varias maneras:
        /// - Especificando el tipo fuente y el nombre de la propiedad.
        /// - Usando una instancia del objeto y el nombre de la propiedad.
        /// - Directamente desde un objeto <see cref="PropertyInfo"/>.
        /// - Mediante una expresión lambda que accede a la propiedad.
        /// El nombre corto se obtiene del atributo <see cref="DisplayAttribute.ShortName"/>. Si no está presente,
        /// se devuelve el nombre de la propiedad.
        /// </remarks>
        /// <param name="Source">El tipo de la clase que contiene la propiedad.</param>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>El nombre corto para mostrar de la propiedad; o el nombre de la propiedad si no se encuentra uno específico.</returns>
        public static string DataAnnotationsDisplayShortName(this Type Source, string propertyName)
            => DataAnnotationsHelpers.GetShortNameCore(Source, propertyName);

        /// <summary>
        /// Proporciona el nombre corto para mostrar de una propiedad.
        /// </summary>
        /// <typeparam name="TSource">El tipo del objeto que contiene la propiedad.</typeparam>
        /// <param name="source">La instancia del objeto.</param>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>El nombre corto para mostrar de la propiedad; o el nombre de la propiedad si no se encuentra uno específico.</returns>
        public static string DataAnnotationsDisplayShortName<TSource>(this TSource source, string propertyName)
            => DataAnnotationsHelpers.GetShortNameCore(source?.GetType() ?? typeof(TSource), propertyName);

        /// <summary>
        /// Proporciona el nombre corto para mostrar de una propiedad.
        /// </summary>
        /// <param name="pi">El objeto <see cref="PropertyInfo"/> que representa la propiedad.</param>
        /// <returns>El nombre corto para mostrar de la propiedad; o el nombre de la propiedad si no se encuentra uno específico.</returns>
        public static string DataAnnotationsDisplayShortName(this PropertyInfo pi)
        {
            DisplayAttribute? ret = DataAnnotationsHelpers.GetAttribCore<DisplayAttribute>(pi);
            return ret?.GetShortName() ?? pi.Name;
        }

        /// <summary>
        /// Proporciona el nombre corto para mostrar de una propiedad.
        /// </summary>
        /// <typeparam name="TSource">El tipo de la clase que contiene la propiedad.</typeparam>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="propertyAccesor">Una expresión lambda que accede a la propiedad (ej. `x => x.PropertyName`).</param>
        /// <returns>El nombre corto para mostrar de la propiedad; o el nombre de la propiedad si no se encuentra uno específico.</returns>
        public static string DataAnnotationsDisplayShortName<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyAccesor)
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(propertyAccesor.Body);
            return DataAnnotationsHelpers.GetShortNameCore(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Permite extraer un atributo específico de una propiedad.
        /// </summary>
        /// <remarks>
        /// Este método y sus sobrecargas permiten obtener una instancia de un atributo decorando una propiedad:
        /// - Especificando el tipo fuente y el nombre de la propiedad.
        /// - Mediante una expresión lambda que accede a la propiedad.
        /// - Directamente desde un objeto <see cref="PropertyInfo"/> (usando `GetCustomAttribute`).
        /// </remarks>
        /// <typeparam name="TSource">El tipo de objeto en el cual se busca la propiedad.</typeparam>
        /// <typeparam name="TAttrib">El tipo del atributo que se busca.</typeparam>
        /// <param name="propertyName">
        /// Nombre de la propiedad. Soporta nombres de propiedades anidadas separadas por puntos (ej. "PropiedadCompleja.PropiedadAnidada").
        /// </param>
        /// <returns>Una instancia del atributo si se encuentra; de lo contrario, <c>null</c>.</returns>
        public static TAttrib? GetAttribute<TSource, TAttrib>(string propertyName)
            where TAttrib : Attribute => DataAnnotationsHelpers.GetAttribCore<TSource, TAttrib>(propertyName);

        /// <summary>
        /// Permite extraer un atributo específico de una propiedad a través de una expresión lambda.
        /// </summary>
        /// <typeparam name="TAttrib">El tipo del atributo que se busca.</typeparam>
        /// <typeparam name="TSource">El tipo de objeto en el cual se busca la propiedad.</typeparam>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="PropertyAccesor">Una expresión lambda que accede a la propiedad (ej. `x => x.PropertyName`).</param>
        /// <returns>Una instancia del atributo si se encuentra; de lo contrario, <c>null</c>.</returns>
        public static TAttrib? GetAttribute<TAttrib, TSource, TProperty>(this Expression<Func<TSource, TProperty>> PropertyAccesor)
      where TAttrib : Attribute
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(PropertyAccesor.Body);
            // El cuerpo de la expresión podría no ser MemberExpression si la expresión es inválida.
            // Por ejemplo, si se pasa una constante o una llamada a método.
            if (mi == null) return null;
            return DataAnnotationsHelpers.GetAttribCore<TSource, TAttrib>(mi.Name);
        }

        /// <summary>
        /// Permite extraer un atributo específico de una propiedad a través de un objeto <see cref="PropertyInfo"/>.
        /// </summary>
        /// <typeparam name="TAttrib">El tipo del atributo que se busca.</typeparam>
        /// <param name="propertyInfo">El objeto <see cref="PropertyInfo"/> que representa la propiedad.</param>
        /// <returns>Una instancia del atributo si se encuentra; de lo contrario, <c>null</c>.</returns>
        public static TAttrib? GetCustomAttribute<TAttrib>(this PropertyInfo propertyInfo)
            where TAttrib : Attribute
            => DataAnnotationsHelpers.GetCustomAttributeCore<TAttrib>(propertyInfo);

        #endregion Methods
    }
}
