using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Provee métodos de extensión para optimizar la notificación de cambios en propiedades
    /// mediante la implementación de <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public static class PropertyChangedExtensions
    {
        private static readonly ConcurrentDictionary<Type, Action<object, string>?> _onPropertyChangedCache = new();

        /// <summary>
        /// Establece el valor de una propiedad, comprueba si ha cambiado y notifica el cambio.
        /// </summary>
        /// <typeparam name="T">Tipo de la propiedad.</typeparam>
        /// <param name="source">La instancia que implementa <see cref="INotifyPropertyChanged"/>.</param>
        /// <param name="field">Referencia al campo subyacente que almacena el valor.</param>
        /// <param name="newValue">El nuevo valor a asignar a la propiedad.</param>
        /// <param name="propertyName">Nombre de la propiedad que cambió. Se obtiene automáticamente mediante <see cref="CallerMemberNameAttribute"/>.</param>
        /// <returns>
        /// <see langword="true"/> si el valor cambió y se realizó la notificación; 
        /// <see langword="false"/> si el nuevo valor es igual al actual o si <paramref name="source"/> es nulo.
        /// </returns>
        public static bool SetValue<T>(
            this INotifyPropertyChanged source,
            ref T field,
            T newValue,
            [CallerMemberName] string propertyName = null!)
        {
            if (source == null) return false;

            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            source.RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifica que una propiedad ha cambiado invocando el método 'OnPropertyChanged' de la instancia.
        /// Utiliza una caché de delegados compilados para maximizar el rendimiento y evitar el costo de reflexión repetitiva.
        /// </summary>
        /// <param name="source">La instancia que dispara la notificación.</param>
        /// <param name="propertyName">Nombre de la propiedad que ha cambiado.</param>
        public static void RaisePropertyChanged(this INotifyPropertyChanged source, string propertyName)
        {
            if (source == null || string.IsNullOrEmpty(propertyName)) return;

            var type = source.GetType();
            var invoker = _onPropertyChangedCache.GetOrAdd(type, t =>
            {
                // Busca el método OnPropertyChanged(string) (puede ser protected o public)
                var method = t.GetMethod("OnPropertyChanged",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new[] { typeof(string) }, null);

                if (method == null) return null;

                // Generamos un delegado Action<object, string> compilado que llama al método específico del tipo.
                // Esto es significativamente más rápido que method.Invoke().
                var instanceParam = Expression.Parameter(typeof(object), "instance");
                var nameParam = Expression.Parameter(typeof(string), "name");
                var castInstance = Expression.Convert(instanceParam, t);
                var call = Expression.Call(castInstance, method, nameParam);
                
                return Expression.Lambda<Action<object, string>>(call, instanceParam, nameParam).Compile();
            });

            invoker?.Invoke(source, propertyName);
        }
    }
}
