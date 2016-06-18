using System.Windows.Input;
using PettingZoo.Infrastructure;
using PettingZoo.Model;

namespace PettingZoo.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IConnectionInfoBuilder connectionInfoBuilder;
        private readonly IConnectionFactory connectionFactory;

        private ConnectionInfo connectionInfo;
        private IConnection connection;

        private readonly DelegateCommand connectCommand;
        private readonly DelegateCommand disconnectCommand;
        private readonly DelegateCommand clearCommand;


        public ConnectionInfo ConnectionInfo {
            get
            {
                return connectionInfo;
            }
            private set
            {
                if (value != connectionInfo)
                {
                    connectionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand ConnectCommand { get { return connectCommand; } }
        public ICommand DisconnectCommand { get { return disconnectCommand; } }
        public ICommand ClearCommand { get { return clearCommand; } }


        public MainViewModel(IConnectionInfoBuilder connectionInfoBuilder, IConnectionFactory connectionFactory)
        {
            this.connectionInfoBuilder = connectionInfoBuilder;
            this.connectionFactory = connectionFactory;

            connectCommand = new DelegateCommand(ConnectExecute);
            disconnectCommand = new DelegateCommand(DisconnectExecute, DisconnectCanExecute);
            clearCommand = new DelegateCommand(ClearExecute, ClearCanExecute);
        }


        private void ConnectExecute()
        {
            var newInfo = connectionInfoBuilder.Build();
            if (newInfo == null) 
                return;

            ConnectionInfo = newInfo;
            connection = connectionFactory.CreateConnection(connectionInfo);

            disconnectCommand.RaiseCanExecuteChanged();
        }


        private void DisconnectExecute()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }

            ConnectionInfo = null;
        }


        private bool DisconnectCanExecute()
        {
            return connection != null;
        }


        private void ClearExecute()
        {
        }


        private bool ClearCanExecute()
        {
            return false;
        }
    }
}