using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PettingZoo.Infrastructure;
using PettingZoo.Model;
using PettingZoo.Properties;

namespace PettingZoo.ViewModel
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        private readonly TaskScheduler uiScheduler;
        private readonly IConnectionInfoBuilder connectionInfoBuilder;
        private readonly IConnectionFactory connectionFactory;

        private ConnectionInfo connectionInfo;
        private IConnection connection;
        private string connectionStatus;
        private readonly ObservableCollection<MessageInfo> messages;
        private MessageInfo selectedMessage;

        private readonly DelegateCommand connectCommand;
        private readonly DelegateCommand disconnectCommand;
        private readonly DelegateCommand clearCommand;


        public ConnectionInfo ConnectionInfo {
            get { return connectionInfo; }
            private set
            {
                if (value == connectionInfo) 
                    return;

                connectionInfo = value;
                RaisePropertyChanged();
            }
        }

        public string ConnectionStatus
        {
            get { return connectionStatus; }
            private set
            {
                if (value == connectionStatus)
                    return;

                connectionStatus = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MessageInfo> Messages { get { return messages; } }

        public MessageInfo SelectedMessage
        {
            get { return selectedMessage; }
            set
            {
                if (value == selectedMessage)
                    return;

                selectedMessage = value;
                RaisePropertyChanged();
                RaiseOtherPropertyChanged("SelectedMessageBody");
                RaiseOtherPropertyChanged("SelectedMessageProperties");
            }
        }

        public string SelectedMessageBody
        {
            get
            {
                return SelectedMessage != null
                    ? MessageBodyRenderer.Render(SelectedMessage.Body, SelectedMessage.ContentType)
                    : "";
            }
        }

        public Dictionary<string, string> SelectedMessageProperties
        {
            get { return SelectedMessage != null ? SelectedMessage.Properties : null; }
        }

        public ICommand ConnectCommand { get { return connectCommand; } }
        public ICommand DisconnectCommand { get { return disconnectCommand; } }
        public ICommand ClearCommand { get { return clearCommand; } }


        public MainViewModel(IConnectionInfoBuilder connectionInfoBuilder, IConnectionFactory connectionFactory)
        {
            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            this.connectionInfoBuilder = connectionInfoBuilder;
            this.connectionFactory = connectionFactory;

            connectionStatus = GetConnectionStatus(null);
            messages = new ObservableCollection<MessageInfo>();

            connectCommand = new DelegateCommand(ConnectExecute);
            disconnectCommand = new DelegateCommand(DisconnectExecute, DisconnectCanExecute);
            clearCommand = new DelegateCommand(ClearExecute, ClearCanExecute);
        }


        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }


        private void ConnectExecute()
        {
            var newInfo = connectionInfoBuilder.Build();
            if (newInfo == null) 
                return;

            if (connection != null)
                connection.Dispose();

            ConnectionInfo = newInfo;
            connection = connectionFactory.CreateConnection(connectionInfo);
            connection.MessageReceived += ConnectionMessageReceived;
            connection.StatusChanged += ConnectionStatusChanged;

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
            ConnectionStatus = GetConnectionStatus(null);

            disconnectCommand.RaiseCanExecuteChanged();
        }


        private bool DisconnectCanExecute()
        {
            return connection != null;
        }


        private void ClearExecute()
        {
            messages.Clear();
            clearCommand.RaiseCanExecuteChanged();
        }


        private bool ClearCanExecute()
        {
            return messages.Count > 0;
        }


        private void ConnectionStatusChanged(object sender, StatusChangedEventArgs args)
        {
            ConnectionStatus = GetConnectionStatus(args);
        }


        private void ConnectionMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            RunFromUiScheduler(() =>
            {
                messages.Add(args.MessageInfo);
                clearCommand.RaiseCanExecuteChanged();
            });            
        }


        private string GetConnectionStatus(StatusChangedEventArgs args)
        {
            if (args != null)
                switch (args.Status)
                {
                    case Model.ConnectionStatus.Connecting:
                        return String.Format(Resources.StatusConnecting, args.Context);

                    case Model.ConnectionStatus.Connected:
                        return String.Format(Resources.StatusConnected, args.Context);

                    case Model.ConnectionStatus.Error:
                        return String.Format(Resources.StatusError, args.Context);
                }

            return Resources.StatusDisconnected;
        }


        private void RunFromUiScheduler(Action action)
        {
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
        }
    }
}