using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Command
{
    internal class RelayCommandCollection<TViewModel> : IReadOnlyDictionary<string, IViewModelCommand>
        where TViewModel : class
    {
        private readonly Dictionary<string, IViewModelCommand> _internal = [];

        public int Count => _internal.Count;

        public IEnumerable<string> Keys => _internal.Keys;

        public IEnumerable<IViewModelCommand> Values => _internal.Values;

        public IViewModelCommand this[string key] => _internal[key];

        public bool ContainsKey(string key) => _internal.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, IViewModelCommand>> GetEnumerator() => _internal.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();
        public void Clear()
        { _internal.Clear(); }
        public bool TryGetValue(string key, out IViewModelCommand value) => _internal.TryGetValue(key, out value!);

        public void AddCommand(OnUpdateButton onUpdateButton, TViewModel viewModel,
            Expression<Action<TViewModel>> executeExpression,
            Expression<Func<TViewModel, bool>>? canExecuteExpression = null)
        {
            RelayCommand<TViewModel> command = new RelayCommand<TViewModel>(viewModel, 
                executeExpression, 
                canExecuteExpression);
            CommandContainer cc=new CommandContainer(command, onUpdateButton);
        }
    }
    public class CommandContainer
    {
        private IViewModelCommand Command { get; }
        private OnUpdateButton OnCanExecute { get; }
        public CommandContainer(IViewModelCommand command, 
            OnUpdateButton onUpdateButton)
        {
            this.Command = command;
            this.OnCanExecute = onUpdateButton;
            RegisterCommand();
        }

        private void RegisterCommand()
        {
            Command.CanExecuteChanged += OnCanExecuteChanged;
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            OnCanExecute.Invoke(Command.CanExecute(null));
        }
    }
public delegate void OnUpdateButton(bool canExecute);
}