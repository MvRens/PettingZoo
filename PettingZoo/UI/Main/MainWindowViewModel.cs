using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.UI.Connection;
using PettingZoo.UI.Subscribe;
using PettingZoo.UI.Tab;

namespace PettingZoo.UI.Main
{
    public class MainWindowViewModel : BaseViewModel, IAsyncDisposable
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnectionDialog connectionDialog;
        private readonly ISubscribeDialog subscribeDialog;
        private readonly ITabFactory tabFactory;

        private ConnectionDialogParams? connectionDialogParams;
        private SubscribeDialogParams? subscribeDialogParams;
        private IConnection? connection;
        private string connectionStatus;
        private ITab? activeTab;

        private readonly DelegateCommand connectCommand;
        private readonly DelegateCommand disconnectCommand;
        private readonly DelegateCommand publishCommand;
        private readonly DelegateCommand subscribeCommand;
        private readonly DelegateCommand closeTabCommand;


        public string ConnectionStatus
        {
            get => connectionStatus;
            private set => SetField(ref connectionStatus, value);
        }
        
        
        public ObservableCollection<ITab> Tabs { get; }

        public ITab? ActiveTab
        {
            get => activeTab;
            set => SetField(ref activeTab, value, otherPropertiesChanged: new []
            {
                nameof(ToolbarCommands), 
                nameof(ToolbarCommandsSeparatorVisibility)
            });
        }

        public ICommand ConnectCommand => connectCommand;
        public ICommand DisconnectCommand => disconnectCommand;
        public ICommand PublishCommand => publishCommand;
        public ICommand SubscribeCommand => subscribeCommand;
        public ICommand CloseTabCommand => closeTabCommand;

        public IEnumerable<TabToolbarCommand> ToolbarCommands => ActiveTab is ITabToolbarCommands tabToolbarCommands 
            ? tabToolbarCommands.ToolbarCommands 
            : Enumerable.Empty<TabToolbarCommand>();

        public Visibility ToolbarCommandsSeparatorVisibility =>
            ToolbarCommands.Any() ? Visibility.Visible : Visibility.Collapsed;


        public MainWindowViewModel(IConnectionFactory connectionFactory, IConnectionDialog connectionDialog, 
            ISubscribeDialog subscribeDialog, ITabFactory tabFactory)
        {
            this.connectionFactory = connectionFactory;
            this.connectionDialog = connectionDialog;
            this.subscribeDialog = subscribeDialog;
            this.tabFactory = tabFactory;

            connectionStatus = GetConnectionStatus(null);

            Tabs = new ObservableCollection<ITab>();
            connectCommand = new DelegateCommand(ConnectExecute);
            disconnectCommand = new DelegateCommand(DisconnectExecute, IsConnectedCanExecute);
            publishCommand = new DelegateCommand(PublishExecute, IsConnectedCanExecute);
            subscribeCommand = new DelegateCommand(SubscribeExecute, IsConnectedCanExecute);
            closeTabCommand = new DelegateCommand(CloseTabExecute, CloseTabCanExecute);
        }


        public async ValueTask DisposeAsync()
        {
            if (connection != null)
                await connection.DisposeAsync();
        }

        
        private async void ConnectExecute()
        {
            var newParams = connectionDialog.Show(connectionDialogParams);

            // TODO support command-line parameters for easier testing
            // var newParams = new ConnectionDialogParams("localhost", "/", 5672, "guest", "guest", true, "test", "#");

            if (newParams == null)
                return;

            if (connection != null)
                await connection.DisposeAsync();

            connectionDialogParams = newParams;
            connection = connectionFactory.CreateConnection(new ConnectionParams(
                connectionDialogParams.Host, connectionDialogParams.VirtualHost, connectionDialogParams.Port,
                connectionDialogParams.Username, connectionDialogParams.Password));
            connection.StatusChanged += ConnectionStatusChanged;

            if (connectionDialogParams.Subscribe)
            {
                var subscriber = connection.Subscribe(connectionDialogParams.Exchange, connectionDialogParams.RoutingKey);
                AddTab(tabFactory.CreateSubscriberTab(CloseTabCommand, subscriber));
                
            }

            ConnectionChanged();
        }


        private async void DisconnectExecute()
        {
            Tabs.Clear();
            
            if (connection != null)
            {
                await connection.DisposeAsync();
                connection = null;
            }

            connectionDialogParams = null;
            ConnectionStatus = GetConnectionStatus(null);

            ConnectionChanged();
        }


        private void SubscribeExecute()
        {
            if (connection == null)
                return;
            
            var newParams = subscribeDialog.Show(subscribeDialogParams);
            if (newParams == null)
                return;

            subscribeDialogParams = newParams;
            
            var subscriber = connection.Subscribe(subscribeDialogParams.Exchange, subscribeDialogParams.RoutingKey);
            AddTab(tabFactory.CreateSubscriberTab(CloseTabCommand, subscriber));
        }


        private void PublishExecute()
        {
            if (connection == null)
                return;
            
            AddTab(tabFactory.CreatePublisherTab(CloseTabCommand, connection));
        }
        

        private bool IsConnectedCanExecute()
        {
            return connection != null;
        }


        private void CloseTabExecute()
        {
            if (ActiveTab == null)
                return;

            var activeTabIndex = Tabs.IndexOf(ActiveTab);
            if (activeTabIndex == -1)
                return;

            Tabs.RemoveAt(activeTabIndex);
            
            if (activeTabIndex == Tabs.Count)
                activeTabIndex--;

            ActiveTab = activeTabIndex >= 0 ? Tabs[activeTabIndex] : null;
            closeTabCommand.RaiseCanExecuteChanged();
        }

        
        private bool CloseTabCanExecute()
        {
            return ActiveTab != null;
        }


        private void AddTab(ITab tab)
        {
            Tabs.Add(tab);
            ActiveTab = tab;
            
            closeTabCommand.RaiseCanExecuteChanged();
        }

        
        private void ConnectionChanged()
        {
            disconnectCommand.RaiseCanExecuteChanged();
            subscribeCommand.RaiseCanExecuteChanged();
            publishCommand.RaiseCanExecuteChanged();
        }

        private void ConnectionStatusChanged(object? sender, StatusChangedEventArgs args)
        {
            ConnectionStatus = GetConnectionStatus(args);
        }



        private static string GetConnectionStatus(StatusChangedEventArgs? args)
        {
            return args?.Status switch
            {
                Core.Connection.ConnectionStatus.Connecting => string.Format(MainWindowStrings.StatusConnecting, args.Context),
                Core.Connection.ConnectionStatus.Connected => string.Format(MainWindowStrings.StatusConnected, args.Context),
                Core.Connection.ConnectionStatus.Error => string.Format(MainWindowStrings.StatusError, args.Context),
                Core.Connection.ConnectionStatus.Disconnected => MainWindowStrings.StatusDisconnected,
                _ => MainWindowStrings.StatusDisconnected
            };
        }
    }
    
    
    public class DesignTimeMainWindowViewModel : MainWindowViewModel
    {
        public DesignTimeMainWindowViewModel() : base(null!, null!, null!, null!)
        {
        }
    }
}