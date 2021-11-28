using System;
using System.Windows.Input;

namespace PettingZoo.UI
{
    public class DelegateCommand<T> : ICommand where T : class?
    {
        private readonly Func<T?, bool>? canExecute;
        private readonly Action<T?> execute;

        public event EventHandler? CanExecuteChanged;


        public DelegateCommand(Action<T?> execute) : this(execute, null)
        {
        }

        public DelegateCommand(Action<T?> execute, Func<T?, bool>? canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }


        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute((T?)parameter);
        }


        public void Execute(object? parameter)
        {
            execute((T?)parameter);
        }


        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }



    public class DelegateCommand : ICommand
    {
        private readonly Func<bool>? canExecute;
        private readonly Action execute;
 
        public event EventHandler? CanExecuteChanged;
 

        public DelegateCommand(Action execute) : this(execute, null) { }
 
        public DelegateCommand(Action execute, Func<bool>? canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
 

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute();
        }
 

        public void Execute(object? parameter)
        {
            execute();
        }
 

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
