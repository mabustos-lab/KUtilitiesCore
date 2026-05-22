using System;
using System.ComponentModel;
using System.Diagnostics;
using KUtilitiesCore.MVVM;

namespace KUtilitiesCore.MVVMTests.EventCommandBinder.Models
{
    /// <summary>
    /// TestViewModel serves as a concrete implementation of ViewModelHelperBase to test
    /// the EventCommandBinderCollection's ability to bind UI events to ViewModel actions.
    /// It includes various methods and properties that mimic real-world command scenarios,
    /// such as execution counting, state-based enabling (CanExecute), and parameter passing.
    /// </summary>
    public class TestViewModel : ViewModelHelperBase
    {
        private int _executeCount;
        private int _secondaryExecuteCount;
        private bool _isToggleEnabled;

        /// <summary>
        /// Tracked manually to verify that the command associated with ExecuteAction was called.
        /// </summary>
        public int ExecuteCount 
        { 
            get => _executeCount; 
            private set => _executeCount = value; 
        }

        /// <summary>
        /// Tracked manually to verify that a secondary command (e.g., from a different event) was called.
        /// </summary>
        public int SecondaryExecuteCount 
        { 
            get => _secondaryExecuteCount; 
            private set => _secondaryExecuteCount = value; 
        }

        /// <summary>
        /// A property used to test conditional execution (CanExecute logic).
        /// Uses SetVMValue to trigger property change notifications, which in turn can trigger
        /// UpdateCommands() via the base class, allowing EventCommandBinder to refresh button states.
        /// </summary>
        public bool IsToggleEnabled
        {
            get => _isToggleEnabled;
            set => this.SetVMValue(ref _isToggleEnabled, value, onPropertyChanged: OnIsToggleEnabledChanged);
        }
        /// <summary>
        /// Muestra en Debug que hubo un cambio
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void OnIsToggleEnabledChanged()
        {
            Debug.WriteLine("IsToggleEnabled: Cambio!!");
        }

        /// <summary>
        /// Stores the last parameter received to verify that EventCommandBinder correctly
        /// passes data (like the ViewModel itself or a specific property) to the command.
        /// </summary>
        public string? LastParam { get; private set; }

        /// <summary>
        /// A property that can be used as a parameter source for command bindings.
        /// </summary>
        public string SomeStringProperty { get; set; } = "DefaultParam";

        /// <summary>
        /// Action method intended to be bound to a command.
        /// </summary>
        public void ExecuteAction()
        {
            ExecuteCount++;
        }

        /// <summary>
        /// Command validation method. Follows the "Can" + MethodName convention.
        /// EventCommandBinderCollection often uses this pattern to determine if a bound event should trigger.
        /// </summary>
        public bool CanExecuteAction()
        {
            return IsToggleEnabled;
        }

        /// <summary>
        /// Secondary action method for verifying multiple bindings in one ViewModel.
        /// </summary>
        public void ExecuteSecondary()
        {
            SecondaryExecuteCount++;
        }

        /// <summary>
        /// Action method that accepts a parameter, used to verify parameter binding.
        /// </summary>
        public void ExecuteWithParam(string param)
        {
            LastParam = param;
        }

        /// <summary>
        /// Utility method to alternate the state of IsToggleEnabled and trigger UI updates.
        /// </summary>
        public void ToggleState()
        {
            IsToggleEnabled = !IsToggleEnabled;
        }

        // --- ViewModelHelperBase Abstract Implementations ---

        public override void OnLoaded()
        {
            // No-op for test purposes
        }

        public override void OnDestroy()
        {
            // No-op for test purposes
        }

        protected override void UpdateCommands()
        {
            // Logic to update command states if needed.
            // In a real VM, this might manually call RaiseCanExecuteChanged for specific RelayCommands.
        }

        protected override void OnClose(CancelEventArgs e)
        {
            // No-op for test purposes
        }
    }
}
