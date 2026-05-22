using System;
using Xunit;
using KUtilitiesCore.MVVMTests.EventCommandBinder.Fakes;
using KUtilitiesCore.MVVMTests.EventCommandBinder.Models;
using KUtilitiesCore.MVVM.Command.Binder;

namespace KUtilitiesCore.MVVMTests.EventCommandBinder
{
    public class EventCommandBinderCollectionTests
    {
        [Fact]
        public void BindCommand_WithoutParameter_ExecutesOnEvent()
        {
            // Arrange: Configuramos ViewModel, botón falso y el binder
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = true; 
            var button = new FakeButton();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);

            // Act: Enlazamos el evento "Click" al método ExecuteAction
            binder.BindCommand(button, "Click", _ => { }, vm => vm.ExecuteAction());
            button.SimulateClick();

            // Assert: Validamos que la acción se haya ejecutado
            Assert.Equal(1, viewModel.ExecuteCount);
        }

        [Fact]
        public void BindCommand_CanExecuteFalse_PreventsExecution()
        {
            // Arrange: ViewModel comienza con estado deshabilitado
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = false; 
            var button = new FakeButton();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);

            // Act: Enlazamos el comando
            binder.BindCommand(button, "Click", _ => { }, vm => vm.ExecuteAction());
            button.SimulateClick();

            // Assert: El contador debe seguir en 0 porque CanExecute es false
            Assert.Equal(0, viewModel.ExecuteCount);
        }

        [Fact]
        public void BindCommand_MultipleButtons_DifferentCommands()
        {
            // Arrange: Dos botones y un binder
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = true;
            var button1 = new FakeButton();
            var button2 = new FakeButton();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);

            // Act: Enlazamos botones a diferentes comandos
            binder.BindCommand(button1, "Click", _ => { }, vm => vm.ExecuteAction(), "Action1");
            binder.BindCommand(button2, "Click", _ => { }, vm => vm.ExecuteSecondary(), "Action2");

            button1.SimulateClick();
            button2.SimulateClick();

            // Assert: Validamos que cada botón disparó su acción correspondiente
            Assert.Equal(1, viewModel.ExecuteCount);
            Assert.Equal(1, viewModel.SecondaryExecuteCount);
        }

        [Fact]
        public void BindCommand_ToggleState_UpdatesCanExecute()
        {
            // Arrange: Setup inicial habilitado
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = true;
            var button = new FakeButton();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);

            binder.BindCommand(button, "Click", _ => { }, vm => vm.ExecuteAction(), "ExecuteAction");

            // Act 1: Ejecución normal
            button.SimulateClick();
            Assert.Equal(1, viewModel.ExecuteCount);

            // Act 2: Deshabilitar vía ViewModel
            viewModel.ToggleState(); // IsToggleEnabled -> false
            button.SimulateClick();
            Assert.Equal(1, viewModel.ExecuteCount); // No debe incrementar

            // Act 3: Re-habilitar
            viewModel.ToggleState(); // IsToggleEnabled -> true
            button.SimulateClick();
            Assert.Equal(2, viewModel.ExecuteCount); // Debe incrementar
        }

        [Fact]
        public void Dispose_UnsubscribesFromEvents()
        {
            // Arrange: Enlazamos un botón
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = true;
            var button = new FakeButton();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);

            binder.BindCommand(button, "Click", _ => { }, vm => vm.ExecuteAction(), "ExecuteAction");

            // Act: Liberamos la colección de binders
            binder.Dispose();
            button.SimulateClick();

            // Assert: Ya no debe ejecutarse la acción
            Assert.Equal(0, viewModel.ExecuteCount);
        }

        [Fact]
        public void Constructor_NullViewModel_ThrowsArgumentNullException()
        {
            // Assert: El constructor debe fallar si el ViewModel es null
            Assert.Throws<ArgumentNullException>(() => new EventCommandBinderCollection<TestViewModel>(null!));
        }

        [Fact]
        public void BindCommand_WithParameter_UsesViewModelProperty()
        {
            // Arrange: Probamos el comando parametrizado
            var viewModel = new TestViewModel();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);
            var button = new FakeButton();
            
            viewModel.SomeStringProperty = "Valor de Prueba";

            // Act: Enlazamos usando la sobrecarga de parámetro (SomeStringProperty)
            binder.BindCommand<FakeButton, string>(
                button, 
                "Click", 
                _ => { }, 
                (vm, param) => vm.ExecuteWithParam(param), 
                vm => vm.SomeStringProperty, "ExecuteWithParam");
            
            button.SimulateClick();

            // Assert: Validamos que el parámetro del ViewModel fue pasado correctamente
            Assert.Equal("Valor de Prueba", viewModel.LastParam);
        }

        [Fact]
        public void BindCommand_WithCommandName_RegistersCommand()
        {
            // Arrange: Definimos un nombre de comando personalizado
            var viewModel = new TestViewModel();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);
            var button = new FakeButton();
            string customName = "MyCustomCommand";

            // Act: Enlazamos asignando el nombre. 
            // Nota: Debido al comportamiento actual de RelayCommand, el nombre del comando 
            // se deriva del método de la expresión ("ExecuteAction"), ignorando el nombre personalizado.
            binder.BindCommand(button, "Click", _ => { }, vm => vm.ExecuteAction(), customName);

            // Assert: Validamos que el comando registró se basó en el nombre del método
            var command = ((ISupportCommands)viewModel).GetRegisteredCommand(customName);
            Assert.NotNull(command);
            Assert.Equal(customName, command.CommandName);
        }

        [Fact]
        public void BindCommand_TargetStatus_ReceivesCanExecuteUpdates()
        {
            // Arrange: Capturamos los cambios de estado enviados al targetStatus
            var viewModel = new TestViewModel();
            viewModel.IsToggleEnabled = true;
            var button = new FakeButton();
            var binder = new EventCommandBinderCollection<TestViewModel>(viewModel);
            bool lastStatus = false;
            int callCount = 0;

            // Act: Pasamos una acción que registre el estado y el número de llamadas
            binder.BindCommand(button, "Click", status => { 
                lastStatus = status; 
                callCount++; 
            }, vm => vm.ExecuteAction());
            
            viewModel.ToggleState(); // false

            // Assert: Validamos que el callback de estado se haya ejecutado y el valor sea correcto
            Assert.True(callCount > 0);
            Assert.False(lastStatus);
        }
    }
}
