using KUtilitiesCore.MVVM.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Extensiones de el ViewModel
    /// </summary>
    public static class ViewModelExtensions
    {
        #region Methods

        /// <summary>
        /// Llama al método con la firma void (protected | public) On{propertyName}Changed() si se
        /// encuentra en la clase.
        /// </summary>
        /// <typeparam name="TSource">Tipo de la fuente.</typeparam>
        /// <param name="source">Objeto de origen.</param>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        public static void Call_OnPropertyChanged<TSource>(this TSource source, string propertyName)
            => GetMethodCore(typeof(TSource), $"On{propertyName}Changed")?.Invoke(source, null);

        /// <summary>
        /// Llama al método con la firma void (protected | public) On{propertyName}Changing() si se
        /// encuentra en la clase.
        /// </summary>
        /// <typeparam name="TSource">Tipo de la fuente.</typeparam>
        /// <param name="source">Objeto de origen.</param>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        public static void Call_OnPropertyChanging<TSource>(this TSource source, string propertyName)
            => GetMethodCore(typeof(TSource), $"On{propertyName}Changing")?.Invoke(source, null);

        /// <summary>
        /// Obtiene la referencia del ViewModel padre que implementa ISupportParentViewModel.
        /// </summary>
        /// <typeparam name="T">Tipo del ViewModel padre.</typeparam>
        /// <param name="source">Objeto fuente.</param>
        /// <returns>Referencia del ViewModel padre.</returns>
        /// <exception cref="ViewModelSourceException">Si el objeto no implementa ISupportParentViewModel.</exception>
        public static T GetParentViewModel<T>(this object source)
            where T : class
        {
            if (!(source is ISupportParentViewModel parentViewModel))
                throw new ViewModelSourceException("El objeto no implementa ISupportParentViewModel.");

            return (T)parentViewModel.ParentViewModel;
        }

        /// <summary>
        /// Notifica cambios en todas las propiedades del ViewModel.
        /// </summary>
        /// <typeparam name="TSource">Tipo del ViewModel.</typeparam>
        /// <param name="source">Objeto fuente.</param>
        public static void RaiseAllPropertiesChanged<TSource>(this TSource source)
            => OnRaisePropertiesChanged(GetViewModel(source));

        /// <summary>
        /// Establece el objeto padre para la clase.
        /// </summary>
        /// <typeparam name="T">Tipo de la clase que implementa ISupportParentViewModel.</typeparam>
        /// <typeparam name="TParent">Tipo del ViewModel padre.</typeparam>
        /// <param name="source">Objeto fuente.</param>
        /// <param name="parent">Objeto padre.</param>
        /// <exception cref="ViewModelSourceException">Si el objeto no implementa ISupportParentViewModel.</exception>
        public static void SetParentViewModel<T, TParent>(this T source, TParent parent)
            where T : class
        {
            if (!(source is ISupportParentViewModel parentViewModel))
                throw new ViewModelSourceException("El objeto no implementa ISupportParentViewModel.");

            parentViewModel.ParentViewModel = parent;
        }

        /// <summary>
        /// Actualiza una propiedad en el ViewModel notificando los cambios y validando la modificación
        /// </summary>
        /// <returns>true si el cambio fue aplicado, false si fue cancelado o no hubo cambios</returns>
        public static bool SetVMValue<TSource, TProperty>(
            this TSource source,
            ref TProperty? currentValue,
            TProperty? newValue,
            OnPropertyChangingDelegate? onPropertyChanging = null,
            Action? onPropertyChanged = null,
            [CallerMemberName] string propertyName = "")
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            // Detección de interfaces en una sola operación
            var isChangeNotifier = source is IViewModelChanged;
            var isChangingNotifier = source is IViewModelChanging;
            
            if (!isChangeNotifier && !isChangingNotifier)
                throw new ViewModelSourceException("El objeto debe implementar al menos una interfaz de notificación");

            if (object.Equals(currentValue, newValue))
                return false;

            var changingArgs = new Helpers.PropertyChangingEventArgs(propertyName, currentValue, newValue);
            onPropertyChanging?.Invoke(source, changingArgs);

            if (changingArgs.Cancel)
                return false;

            // Actualización atómica con notificaciones
            ExecutePropertyUpdate(
                source: source,
                currentValue: ref currentValue,
                newValue: newValue,
                propertyName: propertyName,
                isChangingNotifier: isChangingNotifier,
                isChangeNotifier: isChangeNotifier,
                onPropertyChanged: onPropertyChanged);

            return true;
        }

        private static void ExecutePropertyUpdate<TSource, TProperty>(
       TSource source,
       ref TProperty? currentValue,
       TProperty? newValue,
       string propertyName,
       bool isChangingNotifier,
       bool isChangeNotifier,
       Action? onPropertyChanged)
        {
            // Notificación previa al cambio
            if (isChangingNotifier)
                ((IViewModelChanging)source!).RaisePropertyChanging(propertyName);

            // Actualización del valor
            currentValue = newValue;

            // Notificación post-cambio
            if (isChangeNotifier)
                ((IViewModelChanged)source!).RaisePropertyChanged(propertyName);

            // Callback final
            onPropertyChanged?.Invoke();
        }

        private static MethodInfo GetMethodCore(Type sourceType, string memberName)
        {
            var methodInfo = sourceType.GetMethod(
                memberName,
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            return methodInfo != null && (methodInfo.IsFamily || methodInfo.IsPublic) ? methodInfo : null;
        }

        private static IViewModelHelper GetViewModel<TSource>(TSource source)
        {
            if (source is not IViewModelHelper viewModel)
                throw new ViewModelSourceException("El objeto no implementa IViewModelHelper.");

            return viewModel;
        }

        private static void OnRaisePropertiesChanged(IViewModelHelper viewModelHelper)
        {
            var properties = TypeDescriptor.GetProperties(viewModelHelper)
                .Cast<PropertyDescriptor>()
                .Where(property => !property.IsReadOnly)
                .ToList();

            foreach (var property in properties)
            {
                viewModelHelper.RaisePropertyChanged(property.Name);
            }
        }

        #endregion Methods
    }
}