using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Salamandra.Commands
{
    public class RelayCommandAsync : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Predicate<object?> _canExecute;
        private readonly Action<object?> _onException;

        private bool _isExecuting;
        public bool IsExecuting
        {
            get
            {
                return _isExecuting;
            }
            set
            {
                _isExecuting = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler? CanExecuteChanged;

        public RelayCommandAsync(Func<Task> execute, Predicate<object?> canExecute, Action<object?> onException)
        {
            _execute = execute;
            _canExecute = canExecute;
            _onException = onException;
        }

        public bool CanExecute(object? parameter)
        {
            bool result = _canExecute == null ? true : _canExecute(parameter);
            return this.IsExecuting ? false : result;
        }


        public async void Execute(object? parameter)
        {
            this.IsExecuting = true;

            try
            {
                await ExecuteAsync(parameter);
            }
            catch (Exception ex)
            {
                _onException?.Invoke(ex);
            }

            this.IsExecuting = false;
        }

        public async Task ExecuteAsync(object parameter)
        {
            await this._execute();
        }
    }
}
