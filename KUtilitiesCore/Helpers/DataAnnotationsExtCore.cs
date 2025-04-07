using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using KUtilitiesCore.Properties;
using KUtilitiesCore.Data.DataAnnotations;

namespace KUtilitiesCore.Helpers
{
    internal static class DataAnnotationsExtCore
    {
        #region Methods

        /// <summary>
        /// Obtiene el atributo de un <see cref="PropertyInfo"/>
        /// </summary>
        /// <typeparam name="TAttrib">Tipo de atributo devuelto</typeparam>
        /// <param name="pi">Propiedad de la cual se buscará el atributo</param>
        /// <returns>Regresa la clase del tipo de atributo, si no existe regresa nulo</returns>
        internal static TAttrib GetAttribCore<TAttrib>(this PropertyInfo pi)
            where TAttrib : Attribute => pi.GetCustomAttribute<TAttrib>(true);

        /// <summary>
        /// Obtiene el atributo pasando el Nombre de la propiedad
        /// </summary>
        /// <typeparam name="TSource">Indica el objeto en el cual se buscará la propiedad</typeparam>
        /// <typeparam name="TAttrib">Tipo de atributo a buscar</typeparam>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>El atributo encontrado o nulo si no existe</returns>
        internal static TAttrib GetAttribCore<TSource, TAttrib>(string propertyName)
            where TAttrib : Attribute
        {
            MemberInfo mi = typeof(TSource).GetMember(propertyName).FirstOrDefault();
            if (mi == null) throw new ArgumentException(string.Format(Resources.GetAttribCorePropertyNotFound,
                                                          propertyName,
                                                          typeof(TSource).Name),
                                                          nameof(propertyName));
            return GetAttribCore<TAttrib>(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Obtiene el atributo indicado de una propiedad
        /// </summary>
        /// <typeparam name="TAttrib">Tipo de atributo a buscar</typeparam>
        /// <param name="sourceType">Tipo del objeto que contiene la propiedad</param>
        /// <param name="propertyName">
        /// Nombre de la propiedad, maneja propiedades recursivas separadas por un punto (.)
        /// <code>Obj1.Obj2.Property1</code>
        /// </param>
        /// <returns>El atributo encontrado o nulo si no existe</returns>
        internal static TAttrib GetAttribCore<TAttrib>(Type sourceType, string propertyName)
            where TAttrib : Attribute
        {
            Type parentType = sourceType;
            string correctPropertyName = propertyName;
            if (correctPropertyName.Contains('.'))
            {
                // Indica que es una propiedad recursiva -> Obj1.Obj2.Property1
                string[] properties = propertyName.Split('.');
                correctPropertyName = properties.Last(); // Actualizamos la propiedad que queremos trabajar
                foreach (var sProperty in properties.Take(properties.Length - 1))
                {
                    PropertyInfo pi = parentType.GetProperty(sProperty);
                    parentType = pi.PropertyType;
                }
            }

            MemberInfo mi = parentType.GetMember(correctPropertyName).SingleOrDefault();
            if (mi == null)
                throw new ArgumentException(string.Format(Resources.GetAttribCorePropertyNotFound,
                                                          propertyName,
                                                          sourceType.Name), nameof(propertyName));
            TAttrib result = GetMetadataAttribCore<TAttrib>(parentType, mi.Name);
            if (result != null) return result;
            if (parentType.IsEnum)
            {
                FieldInfo fi = parentType.GetField(mi.Name);
                result = fi.GetCustomAttributes(typeof(TAttrib), true).Cast<TAttrib>().SingleOrDefault();
            }
            else
            {
                PropertyInfo pi = parentType.GetProperty(mi.Name);
                result = GetCustomAttributeCore<TAttrib>(pi);
            }
            return result;
        }

        /// <summary>
        /// Obtiene el <see cref="T:System.Attribute"/> del <see cref="T:System.Reflection.PropertyInfo"/> indicado en el parámetro, si no existe regresa un valor nulo.
        /// </summary>
        /// <typeparam name="TAttribute">Tipo de atributo a buscar</typeparam>
        /// <param name="pi">Información de la propiedad</param>
        /// <returns>El atributo encontrado o nulo si no existe</returns>
        internal static TAttribute GetCustomAttributeCore<TAttribute>(PropertyInfo pi)
            where TAttribute : Attribute => pi.GetCustomAttributes(typeof(TAttribute), true).Cast<TAttribute>().FirstOrDefault();

        #endregion Methods

        #region DataAnnotation Core functions

        /// <summary>
        /// Obtiene la descripción de una propiedad
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto que contiene la propiedad</typeparam>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>La descripción de la propiedad</returns>
        internal static string GetDescriptionCore<TSource>(string propertyName)
            => GetDescriptionCore(typeof(TSource), propertyName);

        /// <summary>
        /// Obtiene la descripción de una propiedad
        /// </summary>
        /// <param name="sourceType">Tipo del objeto que contiene la propiedad</param>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>La descripción de la propiedad</returns>
        internal static string GetDescriptionCore(Type sourceType, string propertyName)
        {
            DisplayAttribute ret = GetAttribCore<DisplayAttribute>(sourceType, propertyName);
            if (ret == null)
            {
                DescriptionAttribute ret2 = GetAttribCore<DescriptionAttribute>(sourceType, propertyName);
                if (ret2 is DescriptionLocalizedAttribute localizedAttribute)
                {
                    return localizedAttribute.GetDescription();
                }
                else if (ret2 != null)
                {
                    return ret2.Description;
                }
                return string.Empty;
            }
            return ret.Description ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el formato de visualización de una propiedad
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto que contiene la propiedad</typeparam>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>El formato de visualización de la propiedad</returns>
        internal static string GetDisplayFormatCore<TSource>(string propertyName)
            => GetDisplayFormatCore(typeof(TSource), propertyName);

        /// <summary>
        /// Obtiene el formato de visualización de una propiedad
        /// </summary>
        /// <param name="sourceType">Tipo del objeto que contiene la propiedad</param>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>El formato de visualización de la propiedad</returns>
        internal static string GetDisplayFormatCore(Type sourceType, string propertyName)
        {
            DisplayFormatAttribute ret = GetAttribCore<DisplayFormatAttribute>(sourceType, propertyName);
            if (ret is DisplayFormatLocalizedAttribute localizedAttribute)
            {
                return localizedAttribute.GetDataFormatString();
            }
            else if (ret != null) return ret.DataFormatString;
            return string.Empty;
        }

        /// <summary>
        /// Obtiene el nombre de visualización de una propiedad
        /// </summary>
        /// <param name="sourceType">Tipo del objeto que contiene la propiedad</param>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>El nombre de visualización de la propiedad</returns>
        internal static string GetDisplayNameCore(Type sourceType, string propertyName)
        {
            DisplayAttribute ret = GetAttribCore<DisplayAttribute>(sourceType, propertyName);
            if (ret == null)
            {
                DisplayNameAttribute ret2 = GetAttribCore<DisplayNameAttribute>(sourceType, propertyName);
                return (ret2 as DisplayNameLocalizedAttribute)?.GetDisplayName()
                    ?? ret2?.DisplayName
                    ?? propertyName;
            }
            return ret.GetName() ?? propertyName;
        }

        /// <summary>
        /// Obtiene el atributo de metadatos de una propiedad
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto que contiene la propiedad</typeparam>
        /// <typeparam name="TAttrib">Tipo de atributo a buscar</typeparam>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>El atributo encontrado o nulo si no existe</returns>
        internal static TAttrib GetMetadataAttribCore<TSource, TAttrib>(string propertyName)
            where TAttrib : Attribute => GetMetadataAttribCore<TAttrib>(typeof(TSource), propertyName);

        /// <summary>
        /// Obtiene el atributo de metadatos de una propiedad
        /// </summary>
        /// <typeparam name="TAttrib">Tipo de atributo a buscar</typeparam>
        /// <param name="sourceType">Tipo del objeto que contiene la propiedad</param>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>El atributo encontrado o nulo si no existe</returns>
        internal static TAttrib GetMetadataAttribCore<TAttrib>(Type sourceType, string propertyName)
            where TAttrib : Attribute
        {
            MetadataTypeAttribute meta = sourceType
                .GetCustomAttributes(false)
                .OfType<MetadataTypeAttribute>()
                .FirstOrDefault();
            if (meta != null)
            {
                PropertyInfo pi = meta.MetadataClassType.GetProperty(propertyName);
                return pi?.GetCustomAttribute<TAttrib>();
            }
            return null;
        }

        /// <summary>
        /// Obtiene el nombre corto de una propiedad
        /// </summary>
        /// <param name="sourceType">Tipo del objeto que contiene la propiedad</param>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>El nombre corto de la propiedad</returns>
        internal static string GetShortNameCore(Type sourceType, string propertyName)
        {
            DisplayAttribute ret = GetAttribCore<DisplayAttribute>(sourceType, propertyName);
            return ret?.GetShortName() ?? propertyName;
        }

        #endregion DataAnnotation Core functions
    }
}
