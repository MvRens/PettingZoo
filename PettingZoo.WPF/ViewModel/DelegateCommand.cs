using System;
using System.Windows.Input;

namespace PettingZoo.WPF.ViewModel
{
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
