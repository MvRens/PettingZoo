using System;
using System.Windows.Input;

namespace PettingZoo.UI.Connection
{
    public class ConnectionDisplayNameViewModel : BaseViewModel
    {
        private string displayName = "";

        private readonly DelegateCommand okCommand;


        public string DisplayName
        {
            get => displayName;
            set => SetField(ref displayName, value, delegateCommandsChanged: new [] { okCommand });
        }

        public ICommand OkCommand => okCommand;

        public event EventHandler? OkClick;


        public ConnectionDisplayNameViewModel()
        {
            okCommand = new DelegateCommand(OkExecute, OkCanExecute);
        }

        
        private void OkExecute()
        {
            OkClick?.Invoke(this, EventArgs.Empty);
        }


        private bool OkCanExecute()
        {
            return !string.IsNullOrWhiteSpace(DisplayName);
        }
    }
}
