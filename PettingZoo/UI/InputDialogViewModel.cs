using System;
using System.Windows.Input;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI
{
    public class InputDialogViewModel : BaseViewModel
    {
        private string title = "";
        private string value = "";

        private readonly DelegateCommand okCommand;


        public string Title
        {
            get => title;
            set => SetField(ref title, value);
        }


        public string Value
        {
            get => value;
            set => SetField(ref this.value, value, delegateCommandsChanged: new [] { okCommand });
        }


        public ICommand OkCommand => okCommand;

        public event EventHandler? OkClick;


        public InputDialogViewModel()
        {
            okCommand = new DelegateCommand(OkExecute, OkCanExecute);
        }

        
        private void OkExecute()
        {
            OkClick?.Invoke(this, EventArgs.Empty);
        }


        private bool OkCanExecute()
        {
            return !string.IsNullOrWhiteSpace(Value);
        }
    }
}
