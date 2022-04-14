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
        private readonly Func<object?, Task> execute;
        private readonly Predicate<object?>? canExecute;
        private readonly Action<object?>? onException;

        private bool isExecuting;
        public bool IsExecuting
        {
            get
            {
                return isExecuting;
            }
            set
            {
                isExecuting = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public RelayCommandAsync(Func<object?, Task> execute, Predicate<object?>? canExecute = null, Action<object?>? onException = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.onException = onException;
        }

        public bool CanExecute(object? parameter)
        {
            bool result = canExecute == null ? true : canExecute(parameter);
            return IsExecuting ? false : result;
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
                onException?.Invoke(ex);
            }

            this.IsExecuting = false;
        }

        private async Task ExecuteAsync(object? parameter)
        {
            await execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
