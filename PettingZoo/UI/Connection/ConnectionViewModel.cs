using System;
using System.Windows.Input;

// TODO validate input

namespace PettingZoo.UI.Connection
{
    public class ConnectionViewModel : BaseViewModel
    {
        private string host;
        private string virtualHost;
        private int port;
        private string username;
        private string password;

        private bool subscribe;
        private string exchange;
        private string routingKey;


        public string Host
        {
            get => host;
            set => SetField(ref host, value);
        }

        public string VirtualHost
        {
            get => virtualHost;
            set => SetField(ref virtualHost, value);
        }

        public int Port
        {
            get => port;
            set => SetField(ref port, value);
        }

        public string Username
        {
            get => username;
            set => SetField(ref username, value);
        }

        public string Password
        {
            get => password;
            set => SetField(ref password, value);
        }


        public bool Subscribe
        {
            get => subscribe;
            set => SetField(ref subscribe, value);
        }

        public string Exchange
        {
            get => exchange;
            set => SetField(ref exchange, value);
        }

        public string RoutingKey
        {
            get => routingKey;
            set => SetField(ref routingKey, value);
        }


        public ICommand OkCommand { get; }

        public event EventHandler? OkClick;


        public ConnectionViewModel(ConnectionDialogParams model)
        {
            OkCommand = new DelegateCommand(OkExecute, OkCanExecute);
            
            host = model.Host;
            virtualHost = model.VirtualHost;
            port = model.Port;
            username = model.Username;
            password = model.Password;

            subscribe = model.Subscribe;
            exchange = model.Exchange;
            routingKey = model.RoutingKey;
        }


        public ConnectionDialogParams ToModel()
        {
            return new(Host, VirtualHost, Port, Username, Password, Subscribe, Exchange, RoutingKey);
        }


        private void OkExecute()
        {
            OkClick?.Invoke(this, EventArgs.Empty);
        }


        private static bool OkCanExecute()
        {
            return true;
        }
    }


    public class DesignTimeConnectionViewModel : ConnectionViewModel
    {
        public DesignTimeConnectionViewModel() : base(ConnectionDialogParams.Default)
        {
        }
    }
}
