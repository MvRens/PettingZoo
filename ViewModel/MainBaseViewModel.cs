using System.Windows.Input;
using PettingZoo.Infrastructure;
using PettingZoo.Model;

namespace PettingZoo.ViewModel
{
    public class MainBaseViewModel : BaseViewModel
    {
        private readonly IConnectionInfoBuilder connectionInfoBuilder;
        private readonly IConnectionFactory connectionFactory;

        private ConnectionInfo connectionInfo;
        private IConnection connection;


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

        public ICommand ConnectCommand { get; private set; }


        public MainBaseViewModel(IConnectionInfoBuilder connectionInfoBuilder, IConnectionFactory connectionFactory)
        {
            this.connectionInfoBuilder = connectionInfoBuilder;
            this.connectionFactory = connectionFactory;

            ConnectCommand = new DelegateCommand(ConnectExecute);
        }


        protected void ConnectExecute()
        {
            var newInfo = connectionInfoBuilder.Build();
            if (newInfo != null)
            {
                ConnectionInfo = newInfo;
                connection = connectionFactory.CreateConnection(connectionInfo);
            }
        }
    }
}