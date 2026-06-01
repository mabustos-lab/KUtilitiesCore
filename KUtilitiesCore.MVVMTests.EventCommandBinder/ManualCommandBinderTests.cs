using System;
using Xunit;
using KUtilitiesCore.MVVM.Command.Binder;
using KUtilitiesCore.MVVM.Command;
using KUtilitiesCore.MVVMTests.EventCommandBinder.Models;

namespace KUtilitiesCore.MVVMTests.EventCommandBinder
{
    public class ManualCommandBinderTests
    {
        // Mock para simular un control de tercero (ej. DevExpress) con un evento no estándar
        public class FakeDevExpressControl
        {
            public event Action<FakeDevExpressControl, string>? CustomEvent;

            public void RaiseCustomEvent(string payload)
            {
                CustomEvent?.Invoke(this, payload);
            }

            public bool IsEnabled { get; set; } = true;
        }

        [Fact]
        public void ManualCommandBinder_ExecutesCommandOnEvent()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = true;
            var control = new FakeDevExpressControl();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);

            // Enlace manual: definimos cómo suscribir y desuscribir el evento
            Action<FakeDevExpressControl, Action<FakeDevExpressControl, string>> subscribe = (ctrl, handler) => ctrl.CustomEvent += handler;
            Action<FakeDevExpressControl, Action<FakeDevExpressControl, string>> unsubscribe = (ctrl, handler) => ctrl.CustomEvent -= handler;
            
            IViewModelCommand command = RelayCommand<TestViewModel>.Create(viewModel, vm => vm.ExecuteAction());

            // Act
            binder.BindCommand(
                control,
                subscribe,
                unsubscribe,
                status => { },
                command
            );

            control.RaiseCustomEvent("Hola desde DevExpress");

            // Assert
            Assert.Equal(1, viewModel.ExecuteCount);
        }

        [Fact]
        public void ManualCommandBinder_RespectsCanExecute()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = false; // Deshabilitado
            var control = new FakeDevExpressControl();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);

            Action<FakeDevExpressControl, Action<FakeDevExpressControl, string>> subscribe = (ctrl, handler) => ctrl.CustomEvent += handler;
            Action<FakeDevExpressControl, Action<FakeDevExpressControl, string>> unsubscribe = (ctrl, handler) => ctrl.CustomEvent -= handler;
            
            IViewModelCommand command = RelayCommand<TestViewModel>.Create(viewModel, vm => vm.ExecuteAction());

            binder.BindCommand(
                control,
                subscribe,
                unsubscribe,
                status => { },
                command
            );

            // Act
            control.RaiseCustomEvent("Prueba");

            // Assert
            Assert.Equal(0, viewModel.ExecuteCount);
        }

        [Fact]
        public void ManualCommandBinder_UpdatesTargetStatus()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = true;
            var control = new FakeDevExpressControl();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);
            bool reportedStatus = false;

            Action<FakeDevExpressControl, Action<FakeDevExpressControl, string>> subscribe = (ctrl, handler) => ctrl.CustomEvent += handler;
            Action<FakeDevExpressControl, Action<FakeDevExpressControl, string>> unsubscribe = (ctrl, handler) => ctrl.CustomEvent -= handler;
            
            IViewModelCommand command = RelayCommand<TestViewModel>.Create(viewModel, vm => vm.ExecuteAction());

            binder.BindCommand(
                control,
                subscribe,
                unsubscribe,
                status => { reportedStatus = status; },
                command
            );

            // Act: Cambiamos estado en ViewModel
            viewModel.ToggleState(); // isEnabled -> false

            // Assert
            Assert.False(reportedStatus);
        }

        [Fact]
        public void ManualCommandBinder_Dispose_Unsubscribes()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = true;
            var control = new FakeDevExpressControl();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);

            Action<FakeDevExpressControl, Action<FakeDevExpressControl, string>> subscribe = (ctrl, handler) => ctrl.CustomEvent += handler;
            Action<FakeDevExpressControl, Action<FakeDevExpressControl, string>> unsubscribe = (ctrl, handler) => ctrl.CustomEvent -= handler;
            
            IViewModelCommand command = RelayCommand<TestViewModel>.Create(viewModel, vm => vm.ExecuteAction());

            binder.BindCommand(
                control,
                subscribe,
                unsubscribe,
                status => { },
                command
            );

            // Act: Liberar binder
            binder.Dispose();
            control.RaiseCustomEvent("Después de Dispose");

            // Assert
            Assert.Equal(0, viewModel.ExecuteCount);
        }
    }
}
