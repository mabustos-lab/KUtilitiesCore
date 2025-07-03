using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Provee metodos de extension para objetos que implementen <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public static class INotifyPropertychangedEx
    {
        /// <summary>
        /// Actualiza la propiedad solo si el valor cambia e invoca
        /// el método protegido OnPropertyChanged(string) si existe.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto que implementa INotifyPropertyChanged.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="source">Instancia origen.</param>
        /// <param name="currentValue">Campo que almacena el valor actual.</param>
        /// <param name="newValue">Nuevo valor propuesto.</param>
        /// <param name="propertyName">
        /// Se completa automáticamente con el nombre del llamador.
        /// </param>
        /// <returns>
        /// true si el valor cambió y se notificó; false si los valores eran iguales.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Se lanza si <paramref name="source"/> es null.
        /// </exception>
        public static bool SetValueAndNotify<TSource, TProperty>(
            this TSource source,
            ref TProperty currentValue,
            TProperty newValue,
            [CallerMemberName] string propertyName = "")
            where TSource : class, INotifyPropertyChanged
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            // Si no hay cambios, salir.
            if (EqualityComparer<TProperty>.Default.Equals(currentValue, newValue))
                return false;

            // Asigna el nuevo valor.
            currentValue = newValue;

            // Busca OnPropertyChanged(string) mediante reflexión.
            MethodInfo? onPropertyChanged =
                GetMethodCore(typeof(TSource), "OnPropertyChanged");

            // Invoca el método, si existe.
            onPropertyChanged?.Invoke(source, new object[] { propertyName });

            return true;
        }

        /// <summary>
        /// Devuelve el MethodInfo de un método de instancia (público o protegido)
        /// cuyo nombre coincide con <paramref name="memberName"/>.
        /// </summary>
        private static MethodInfo? GetMethodCore(Type sourceType, string memberName) =>
            sourceType.GetMethod(
                memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);
    }
}

