using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PettingZoo.Infrastructure;
using PettingZoo.Model;

namespace PettingZoo.ViewModel
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        private readonly TaskScheduler uiScheduler;
        private readonly IConnectionInfoBuilder connectionInfoBuilder;
        private readonly IConnectionFactory connectionFactory;

        private ConnectionInfo connectionInfo;
        private IConnection connection;
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

            ConnectionInfo = newInfo;
            connection = connectionFactory.CreateConnection(connectionInfo);
            connection.MessageReceived += ConnectionMessageReceived;

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


        private void ConnectionMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            RunFromUiScheduler(() =>
            {
                messages.Add(e.MessageInfo);
                clearCommand.RaiseCanExecuteChanged();
            });            
        }


        private void RunFromUiScheduler(Action action)
        {
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
        }
    }
}