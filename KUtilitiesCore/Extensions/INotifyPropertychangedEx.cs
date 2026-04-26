using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Provee métodos de extensión para objetos que implementen <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    /// <remarks>
    /// Esta clase se mantiene por compatibilidad. Se recomienda utilizar <see cref="PropertyChangedExtensions"/>.
    /// </remarks>
    public static class INotifyPropertychangedEx
    {
        /// <summary>
        /// Actualiza la propiedad solo si el valor cambia e invoca el método OnPropertyChanged(string) si existe.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto que implementa INotifyPropertyChanged.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="source">Instancia origen.</param>
        /// <param name="currentValue">Referencia al campo que almacena el valor actual.</param>
        /// <param name="newValue">Nuevo valor propuesto.</param>
        /// <param name="propertyName">Nombre de la propiedad (se obtiene automáticamente).</param>
        /// <returns>true si el valor cambió y se notificó; false si los valores eran iguales.</returns>
        public static bool SetValueAndNotify<TSource, TProperty>(
            this TSource source,
            ref TProperty currentValue,
            TProperty newValue,
            [CallerMemberName] string propertyName = "")
            where TSource : class, INotifyPropertyChanged
        {
            // Redirige a la implementación optimizada en PropertyChangedExtensions
            return PropertyChangedExtensions.SetValue(source, ref currentValue, newValue, propertyName);
        }
    }
}
