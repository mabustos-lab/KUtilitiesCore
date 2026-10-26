using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.MVVM;
using KUtilitiesCore.MVVM.Helpers;
using System;

namespace KUtilitiesCore.MVVMTests
{
    /// <summary>
    /// Pruebas de las extensiones de ViewModel, en particular las guardas de
    /// <see cref="ViewModelExtensions.GetParentViewModel{T}(object)"/> para el
    /// caso en que el ViewModel padre no está inicializado.
    /// </summary>
    [TestClass()]
    public class ViewModelExtensionsTests
    {
        private class ParentViewModel
        {
        }

        private class ChildViewModel : ISupportParentViewModel
        {
            private object? _parentViewModel;

            public event EventHandler? ParentViewModelChanged;

            public object? ParentViewModel
            {
                get => _parentViewModel;
                set
                {
                    _parentViewModel = value;
                    ParentViewModelChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [TestMethod()]
        public void GetParentViewModel_ReturnsParent_WhenInitialized()
        {
            ChildViewModel child = new() { ParentViewModel = new ParentViewModel() };

            var parent = child.GetParentViewModel<ParentViewModel>();

            Assert.IsInstanceOfType(parent, typeof(ParentViewModel));
        }

        [TestMethod()]
        public void GetParentViewModel_Throws_WhenParentNotInitialized()
        {
            ChildViewModel child = new();

            Assert.ThrowsExactly<ViewModelSourceException>(
                () => child.GetParentViewModel<ParentViewModel>());
        }

        [TestMethod()]
        public void GetParentViewModel_Throws_WhenSourceDoesNotImplementInterface()
        {
            Assert.ThrowsExactly<ViewModelSourceException>(
                () => new object().GetParentViewModel<ParentViewModel>());
        }
    }
}
