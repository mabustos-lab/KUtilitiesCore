using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Helpers
{
    internal static class DataAnnotationsExtCore
    {
        #region Methods

        /// <summary>
        /// Obtiene el atributo de un <see cref="PropertyInfo"/>
        /// </summary>
        /// <typeparam name="TAttrib">Tipo de atrivuto debuelto</typeparam>
        /// <param name="pi">Propiedad de la cual se buscará el atrivuto</param>
        /// <returns>Regresa la clase del tipo de atributo, si no existe regresa nulo</returns>
        internal static TAttrib GetAttribCore<TAttrib>(this PropertyInfo pi)
            where TAttrib : Attribute => pi.GetCustomAttribute<TAttrib>(true);

        /// <summary>
        /// Obtiene el atributo pasando el Nombre de la propiedad
        /// </summary>
        /// <typeparam name="TSource">Indica el objeto en el cuan se vuscara la propiedad</typeparam>
        /// <typeparam name="TAttrib">Tipo de attributo a buscar</typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal static TAttrib GetAttribCore<TSource, TAttrib>(string propertyName)
       where TAttrib : Attribute
        {
            MemberInfo mi = typeof(TSource).GetMember(propertyName).FirstOrDefault<MemberInfo>();
            if (mi == null) throw new ArgumentException(string.Format(Resources.GetAttribCorePropertyNotFound,
                                                          propertyName,
                                                          typeof(TSource).Name),
                                                          nameof(propertyName));
            return GetAttribCore<TAttrib>(typeof(TSource), mi.Name);
        }

        /// <summary>
        /// Obtiene el attibuto indicado de una propiedad
        /// </summary>
        /// <typeparam name="TAttrib"></typeparam>
        /// <param name="sourceType"></param>
        /// <param name="propertyName">
        /// Nombre de la propiedad, maneja propiedades recursivas separadas por un Punto (.)
        /// <code>Obj1.Obj2.Property1</code>
        /// </param>
        /// <returns></returns>
        internal static TAttrib GetAttribCore<TAttrib>(Type sourceType, string propertyName)
       where TAttrib : Attribute
        {
            Type parentType = sourceType;
            string CorrectPropertyName = propertyName;
            if (CorrectPropertyName.Contains('.'))
            {
                //Indica que es una propiedad recursiva -> Obj1.Obj2.Property1
                string[] Properties = propertyName.Split('.');
                CorrectPropertyName = Properties.Last(); //Actualizamos la propiedad que queremos trabajar
                foreach (var sProperty in Properties.Take(Properties.Count() - 1))
                {
                    PropertyInfo pi = parentType.GetProperty(sProperty);
                    parentType = pi.PropertyType;
                }
            }

            MemberInfo mi = parentType.GetMember(CorrectPropertyName).SingleOrDefault();
            if (mi == null)
                throw new ArgumentException(string.Format(Resources.GetAttribCorePropertyNotFound,
                                                          propertyName,
                                                          sourceType.Name), nameof(propertyName));
            TAttrib Result = GetMetadataAttribCore<TAttrib>(parentType, mi.Name);
            if (Result != null) return Result;
            if (parentType.IsEnum)
            {
                FieldInfo fi = parentType.GetField(mi.Name);
                Result = fi.GetCustomAttributes(typeof(TAttrib), true).Cast<TAttrib>().SingleOrDefault();
            }
            else
            {
                PropertyInfo pi = parentType.GetProperty(mi.Name);
                Result = GetCustomAttributeCore<TAttrib>(pi);
            }
            return Result;
        }

        /// <summary>
        /// Obtiene el <see cref="T:System.Attribute"/> del <see
        /// cref="T:System.Reflection.PropertyInfo"/> indicado en el parametro, si no existe regresa
        /// un valor nulo.
        /// </summary>
        internal static TAttribute GetCustomAttributeCore<TAttribute>(PropertyInfo PI)
        where TAttribute : Attribute => PI.GetCustomAttributes(typeof(TAttribute), true).Cast<TAttribute>().FirstOrDefault<TAttribute>();

        #endregion Methods

        #region DataAnnotation Core functions

        internal static string GetDescriptionCore<TSource>(string propertyName)
            => GetDescriptionCore(typeof(TSource), propertyName);

        internal static string GetDescriptionCore(Type sourceType, string propertyName)
        {
            DisplayAttribute ret = GetAttribCore<DisplayAttribute>(sourceType, propertyName);
            if (ret == null)
            {
                DescriptionAttribute ret2 = GetAttribCore<DescriptionAttribute>(sourceType, propertyName);
                if (((DescriptionLocalizedAttribute)ret2) != null)
                {
                    return ((DescriptionLocalizedAttribute)ret2).GetDescription();
                }
                else if (ret2 != null)
                {
                    return ret2.Description;
                }
                return string.Empty;
            }
            return ret?.Description ?? string.Empty;
        }

        internal static string GetDisplayFormatCore<TSource>(string propertyName)
            => GetDisplayFormatCore(typeof(TSource), propertyName);

        internal static string GetDisplayFormatCore(Type SourceType, string propertyName)
        {
            DisplayFormatAttribute ret = GetAttribCore<DisplayFormatAttribute>(SourceType, propertyName);
            if (ret is DisplayFormatLocalizedAttribute ret1)
            {
                return ret1.GetDataFormatString();
            }
            else if (ret != null) return ret.DataFormatString;
            return string.Empty;
        }

        internal static string GetDisplayNameCore(Type SourceType, string propertyName)
        {
            DisplayAttribute ret = GetAttribCore<DisplayAttribute>(SourceType, propertyName);
            if (ret == null)
            {
                DisplayNameAttribute ret2 = GetAttribCore<DisplayNameAttribute>(SourceType, propertyName);
                return ((DisplayNameLocalizedAttribute)ret2)?.GetDisplayName()
                    ?? ret2?.DisplayName
                    ?? propertyName;
            }
            return ret.GetName() ?? propertyName;
        }

        internal static TAttrib GetMetadataAttribCore<TSource, TAttrib>(string propertyName)
                                                    where TAttrib : Attribute => GetMetadataAttribCore<TAttrib>(typeof(TSource), propertyName);

        /// <summary>
        /// Obtiene el <see cref="T:System.Attribute"/> del <see cref="T:System.Type"/> indicado en
        /// el parametro, si no existe regresa un valor nulo.
        /// </summary>
        internal static TAttrib GetMetadataAttribCore<TAttrib>(Type souceType, string propertyName)
        where TAttrib : Attribute
        {
            MetadataTypeAttribute Meta = souceType.
                GetCustomAttributes(false).
                OfType<MetadataTypeAttribute>().FirstOrDefault();
            if (Meta != null)
            {
                PropertyInfo pi = Meta.MetadataClassType.GetProperty(propertyName);
                return pi?.GetCustomAttribute<TAttrib>();
            }
            return null;
        }

        internal static string GetShortNameCore(Type SourceType, string propertyName)
        {
            DisplayAttribute ret = GetAttribCore<DisplayAttribute>(SourceType, propertyName);
            return ret?.GetShortName() ?? propertyName;
        }

        #endregion DataAnnotation Core functions
    }
}
