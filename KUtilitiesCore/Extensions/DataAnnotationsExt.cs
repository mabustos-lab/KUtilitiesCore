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
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario
        /// </summary>
        public static string DataAnnotationsDescription<TSource>(string propertyName)
        {
            return typeof(TSource).DataAnnotationsDescription(propertyName);
        }

        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="propertyName">Nombre de la propiedad que se desea buscar</param>
        /// <returns></returns>
        public static string DataAnnotationsDescription(this Type Source, string propertyName)
            => DataAnnotationsHelpers.GetDescriptionCore(Source, propertyName);

        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario
        /// </summary>
        public static string DataAnnotationsDescription<TSource>(this TSource Source, string propertyName)
            => DataAnnotationsHelpers.GetDescriptionCore(Source.GetType(), propertyName);

        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario
        /// </summary>
        public static string DataAnnotationsDescription(this PropertyInfo pi)
        {
            DisplayAttribute ret = DataAnnotationsHelpers.GetAttribCore<DisplayAttribute>(pi);
            if (ret == null)
            {
                DescriptionAttribute ret2 = DataAnnotationsHelpers.GetAttribCore<DescriptionAttribute>(pi);
                return ret2?.Description ?? "";
            }
            return ret?.GetDescription() ?? "";
        }

        /// <summary>
        /// Proporciona el texto de la propiedad que corresponde a la Descripción que se mostrará al usuario
        /// </summary>
        public static string DataAnnotationsDescription<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyAccesor)
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(propertyAccesor.Body);
            return DataAnnotationsHelpers.GetDescriptionCore(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Proporciona el texto de presentación de una propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayFormat(this Type Source, string propertyName)
             => DataAnnotationsHelpers.GetDisplayFormatCore(Source, propertyName);

        /// <summary>
        /// Proporciona el texto de presentación de una propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayFormat<TSource>(this TSource Source, string propertyName)
            => DataAnnotationsHelpers.GetDisplayFormatCore(Source.GetType(), propertyName);

        /// <summary>
        /// Proporciona el texto de presentación de una propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayFormat(this PropertyInfo pi)
        {
            DisplayFormatAttribute ret = DataAnnotationsHelpers.GetAttribCore<DisplayFormatAttribute>(pi);
            return ret?.DataFormatString ?? pi.Name;
        }

        /// <summary>
        /// Proporciona el texto de presentación de una propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayFormat<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyAccesor)
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(propertyAccesor.Body);
            return DataAnnotationsHelpers.GetDisplayFormatCore(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Proporciona el texto para mostrar de un propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayName<TSource>(this TSource Source, string propertyName)
            => DataAnnotationsHelpers.GetDisplayNameCore(Source.GetType(), propertyName);

        /// <summary>
        /// Proporciona el texto para mostrar de un propiedad.
        /// </summary>
        /// <returns>Regresa el valor que contiene el atributo en DataAnnotations,
        /// de lo contrario el nombre de la propiedad</returns>
        public static string DataAnnotationsDisplayName(this Type SourceType, string propertyName)
            => DataAnnotationsHelpers.GetDisplayNameCore(SourceType, propertyName);

        /// <summary>
        /// Proporciona el texto para mostrar de un propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayName(this PropertyInfo pi)
        {
            DisplayAttribute ret = DataAnnotationsHelpers.GetAttribCore<DisplayAttribute>(pi);
            if (ret == null)
            {
                DisplayNameAttribute ret2 = DataAnnotationsHelpers.GetAttribCore<DisplayNameAttribute>(pi);
                return ret2?.DisplayName ?? pi.Name;
            }
            return ret?.GetName() ?? pi.Name;
        }

        /// <summary>
        /// Proporciona el texto para mostrar de un propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayName<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyAccesor)
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(propertyAccesor.Body);
            return DataAnnotationsHelpers.GetDisplayNameCore(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Proporciona el texto para mostrar de un propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayShortName(this Type Source, string propertyName)
            => DataAnnotationsHelpers.GetShortNameCore(Source, propertyName);

        /// <summary>
        /// Proporciona el texto para mostrar de un propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayShortName<TSource>(this TSource Source, string propertyName)
            => DataAnnotationsHelpers.GetShortNameCore(Source.GetType(), propertyName);

        /// <summary>
        /// Proporciona el texto para mostrar de un propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayShortName(this PropertyInfo pi)
        {
            DisplayAttribute ret = DataAnnotationsHelpers.GetAttribCore<DisplayAttribute>(pi);
            return ret?.GetShortName() ?? pi.Name;
        }

        /// <summary>
        /// Proporciona el texto para mostrar de un propiedad.
        /// </summary>
        public static string DataAnnotationsDisplayShortName<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyAccesor)
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(propertyAccesor.Body);
            return DataAnnotationsHelpers.GetShortNameCore(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Permite extraer un <see cref="Attribute"/> de un propiedad
        /// </summary>
        /// <typeparam name="TSource">El tipo de objeto en el cual se busca la propiedad</typeparam>
        /// <typeparam name="TAttrib">El tipo de propiedad que se busca</typeparam>
        /// <param name="propertyName">
        /// Nombre de la propiedad, soporta recurrencia de propiedades separados por un punto
        /// </param>
        /// <returns></returns>
        public static TAttrib GetAttribute<TSource, TAttrib>(string propertyName)
            where TAttrib : Attribute => DataAnnotationsHelpers.GetAttribCore<TSource, TAttrib>(propertyName);

        /// <summary>
        /// Permite extraer un <see cref="Attribute"/> de un propiedad a travez de uan expresión
        /// </summary>
        /// <typeparam name="TSource">El tipo de objeto en el cual se busca la propiedad</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad</typeparam>
        /// <typeparam name="TAttrib">El tipo de propiedad que se busca</typeparam>
        /// <param name="PropertyAccesor"></param>
        /// <returns></returns>
        public static TAttrib GetAttribute<TAttrib, TSource, TProperty>(this Expression<Func<TSource, TProperty>> PropertyAccesor)
      where TAttrib : Attribute
        {
            MemberInfo mi = ExpressionsHelpers.GetPropertyInformation(PropertyAccesor.Body);
            if (mi == null) return null;
            return DataAnnotationsHelpers.GetAttribCore<TSource, TAttrib>(mi.Name);
        }

        /// <summary>
        /// Permite extraer un <see cref="Attribute"/> de un propiedad a travez de un <see cref="PropertyInfo"/>
        /// </summary>
        /// <typeparam name="TAttrib"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static TAttrib GetCustomAttribute<TAttrib>(this PropertyInfo propertyInfo)
            where TAttrib : Attribute
            => DataAnnotationsHelpers.GetCustomAttributeCore<TAttrib>(propertyInfo);

        #endregion Methods
    }
}
