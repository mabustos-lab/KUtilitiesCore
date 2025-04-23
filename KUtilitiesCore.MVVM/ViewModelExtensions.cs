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
        /// Simplifica la actualización de una propiedad en el ViewModel.
        /// </summary>
        /// <typeparam name="TSource">Tipo del ViewModel.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="source">Objeto fuente.</param>
        /// <param name="oldValue">Valor antiguo de la propiedad.</param>
        /// <param name="newValue">Valor nuevo de la propiedad.</param>
        /// <param name="onPropertyChanging">Delegado para el evento de cambio de propiedad.</param>
        /// <param name="onPropertyChanged">Acción para el evento de cambio de propiedad.</param>
        /// <param name="propertyName">Nombre de la propiedad (impleméntalo con CallerMemberName).</param>
        /// <returns>true si se permitió el cambio, false en caso contrario.</returns>
        public static bool SetVMValue<TSource, TProperty>(
            this TSource source,
            ref TProperty oldValue,
            TProperty newValue,
            OnPropertyChangingDelegate onPropertyChanging = null,
            Action onPropertyChanged = null,
            [CallerMemberName] string propertyName = "")
        {
            IViewModelChanged viewModelChanged = null;
            IViewModelChanging viewModelChanging = null;

            if (source is IViewModelChanged changed) viewModelChanged = changed;
            if (source is IViewModelChanging changing) viewModelChanging = changing;

            if (viewModelChanged == null && viewModelChanging == null)
                throw new ViewModelSourceException("El objeto no implementa IViewModelChanged ni IViewModelChanging.");

            bool allowChange = !Equals(oldValue, newValue);

            if (allowChange)
            {
                var args = new Helpers.PropertyChangingEventArgs(propertyName, oldValue, newValue);

                onPropertyChanging?.Invoke(source, args);
                allowChange = !args.Cancel;

                if (!args.Cancel)
                {
                    if (viewModelChanging != null) viewModelChanging.RaisePropertyChanging(propertyName);
                    oldValue = newValue;
                    if (viewModelChanged != null) viewModelChanged.RaisePropertyChanged(propertyName);
                    onPropertyChanged?.Invoke();
                }
            }

            return allowChange;
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