using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.ExportImport.Subscriber;
using PettingZoo.UI.Connection;
using PettingZoo.UI.Subscribe;
using PettingZoo.UI.Tab;
using PettingZoo.UI.Tab.Subscriber;
using PettingZoo.UI.Tab.Undocked;
using PettingZoo.WPF.ProgressWindow;
using PettingZoo.WPF.ViewModel;
using Serilog;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace PettingZoo.UI.Main
{
    public enum ConnectionStatusType
    {
        Connecting,
        Ok,
        Error
    }


    public class MainWindowViewModel : BaseViewModel, IAsyncDisposable, ITabHost
    {
        private readonly ILogger logger;
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnectionDialog connectionDialog;
        private readonly ISubscribeDialog subscribeDialog;
        private readonly ITabContainer tabContainer;
        private readonly ITabHostProvider tabHostProvider;
        private readonly ITabFactory tabFactory;
        private readonly IExportImportFormatProvider exportImportFormatProvider;

        private SubscribeDialogParams? subscribeDialogParams;
        private IConnection? connection;
        private string connectionStatus;
        private ITab? activeTab;
        private readonly Dictionary<ITab, Window> undockedTabs = new();

        private readonly DelegateCommand connectCommand;
        private readonly DelegateCommand disconnectCommand;
        private readonly DelegateCommand publishCommand;
        private readonly DelegateCommand subscribeCommand;
        private readonly DelegateCommand closeTabCommand;
        private readonly DelegateCommand undockTabCommand;
        private readonly DelegateCommand importCommand;

        private ConnectionStatusType connectionStatusType;

        public Window? TabHostWindow { get; set; }


        public string ConnectionStatus
        {
            get => connectionStatus;
            private set => SetField(ref connectionStatus, value);
        }


        public ConnectionStatusType ConnectionStatusType
        {
            get => connectionStatusType;
            set => SetField(ref connectionStatusType, value, otherPropertiesChanged: new [] { nameof(ConnectionStatusOk), nameof(ConnectionStatusError), nameof(ConnectionStatusConnecting) });
        }

        public Visibility ConnectionStatusOk => ConnectionStatusType == ConnectionStatusType.Ok ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ConnectionStatusError => ConnectionStatusType == ConnectionStatusType.Error ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ConnectionStatusConnecting => ConnectionStatusType == ConnectionStatusType.Connecting ? Visibility.Visible : Visibility.Collapsed;


        public ObservableCollection<ITab> Tabs { get; }

        public ITab? ActiveTab
        {
            get => activeTab;
            set
            {
                var currentTab = activeTab;

                if (!SetField(ref activeTab, value, otherPropertiesChanged: new[] { nameof(ToolbarCommands), nameof(ToolbarCommandsSeparatorVisibility) }))
                    return;

                (currentTab as ITabActivate)?.Deactivate();
                (activeTab as ITabActivate)?.Activate();
            }
        }

        public ICommand ConnectCommand => connectCommand;
        public ICommand DisconnectCommand => disconnectCommand;
        public ICommand PublishCommand => publishCommand;
        public ICommand SubscribeCommand => subscribeCommand;
        public ICommand CloseTabCommand => closeTabCommand;
        public ICommand UndockTabCommand => undockTabCommand;
        public ICommand ImportCommand => importCommand;

        public IEnumerable<TabToolbarCommand> ToolbarCommands => ActiveTab is ITabToolbarCommands tabToolbarCommands 
            ? tabToolbarCommands.ToolbarCommands 
            : Enumerable.Empty<TabToolbarCommand>();

        public Visibility ToolbarCommandsSeparatorVisibility =>
            ToolbarCommands.Any() ? Visibility.Visible : Visibility.Collapsed;

        public Visibility NoTabsVisibility =>
            Tabs.Count > 0 ? Visibility.Collapsed : Visibility.Visible;


        public MainWindowViewModel(ILogger logger, IConnectionFactory connectionFactory, IConnectionDialog connectionDialog, 
            ISubscribeDialog subscribeDialog, ITabContainer tabContainer, ITabHostProvider tabHostProvider, ITabFactory tabFactory,
            IExportImportFormatProvider exportImportFormatProvider)
        {
            tabHostProvider.SetInstance(this);

            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.connectionDialog = connectionDialog;
            this.subscribeDialog = subscribeDialog;
            this.tabContainer = tabContainer;
            this.tabHostProvider = tabHostProvider;
            this.tabFactory = tabFactory;
            this.exportImportFormatProvider = exportImportFormatProvider;

            connectionStatus = GetConnectionStatus(null);
            connectionStatusType = ConnectionStatusType.Error;

            Tabs = new ObservableCollection<ITab>();
            connectCommand = new DelegateCommand(ConnectExecute);
            disconnectCommand = new DelegateCommand(DisconnectExecute, IsConnectedCanExecute);
            publishCommand = new DelegateCommand(PublishExecute, IsConnectedCanExecute);
            subscribeCommand = new DelegateCommand(SubscribeExecute, IsConnectedCanExecute);
            closeTabCommand = new DelegateCommand(CloseTabExecute, HasActiveTabCanExecute);
            undockTabCommand = new DelegateCommand(UndockTabExecute, HasActiveTabCanExecute);
            importCommand = new DelegateCommand(ImportExecute);
        }


        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            if (connection != null)
                await connection.DisposeAsync();
        }

        
        private async void ConnectExecute()
        {
            var connectionSettings = await connectionDialog.Show();
            if (connectionSettings == null)
                return;

            if (connection != null)
                await connection.DisposeAsync();

            connection = connectionFactory.CreateConnection(new ConnectionParams(
                connectionSettings.Host, connectionSettings.VirtualHost, connectionSettings.Port,
                connectionSettings.Username, connectionSettings.Password));
            connection.StatusChanged += ConnectionStatusChanged;

            if (connectionSettings.Subscribe)
            {
                var subscriber = connection.Subscribe(connectionSettings.Exchange, connectionSettings.RoutingKey);
                tabFactory.CreateSubscriberTab(connection, subscriber);
            }

            ConnectionChanged();
        }


        private async void DisconnectExecute()
        {
            Tabs.Clear();

            var capturedUndockedTabs = undockedTabs.ToList();
            undockedTabs.Clear();

            foreach (var undockedTab in capturedUndockedTabs)
                undockedTab.Value.Close();

            RaisePropertyChanged(nameof(NoTabsVisibility));
            undockTabCommand.RaiseCanExecuteChanged();

            if (connection != null)
            {
                await connection.DisposeAsync();
                connection = null;
            }

            ConnectionStatus = GetConnectionStatus(null);
            ConnectionStatusType = ConnectionStatusType.Error;
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
            tabFactory.CreateSubscriberTab(connection, subscriber);
        }


        private void PublishExecute()
        {
            if (connection == null)
                return;
            
            tabFactory.CreatePublisherTab(connection);
        }
        

        private bool IsConnectedCanExecute()
        {
            return connection != null;
        }


        private void CloseTabExecute()
        {
            var tab = RemoveActiveTab();
            (tab as IDisposable)?.Dispose();
        }


        private void UndockTabExecute()
        {
            var tab = RemoveActiveTab();
            if (tab == null)
                return;

            var tabHostWindow = UndockedTabHostWindow.Create(tabHostProvider, tab, tabContainer.TabWidth, tabContainer.TabHeight);
            undockedTabs.Add(tab, tabHostWindow);

            tabHostWindow.Show();
            (tab as ITabHostWindowNotify)?.HostWindowChanged(tabHostWindow);
        }


        private void ImportExecute()
        {
            var formats = exportImportFormatProvider.ImportFormats.ToArray();

            var dialog = new OpenFileDialog
            {
                Filter = string.Join('|', formats.Select(f => f.Filter))
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (dialog.FilterIndex <= 0 || dialog.FilterIndex > formats.Length)
                return;

            var filename = dialog.FileName;
            var format = formats[dialog.FilterIndex - 1];

            var progressWindow = new ProgressWindow(MainWindowStrings.ImportProgressWindowTitle)
            {
                Owner = TabHostWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            progressWindow.Show();


            Task.Run(async () =>
            {
                try
                {
                    IReadOnlyList<ReceivedMessageInfo> messages;
                    await using (var importFile =
                                 new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        messages = await format.Import(
                            new StreamProgressDecorator(importFile, progressWindow).Stream,
                            progressWindow.CancellationToken);
                    }

                    if (progressWindow.CancellationToken.IsCancellationRequested)
                        return;

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        progressWindow.Close();
                        progressWindow = null;

                        tabFactory.CreateSubscriberTab(connection, new ImportSubscriber(filename, messages));
                    });
                }
                catch (OperationCanceledException)
                {
                    // User cancelled
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error while importing messages");

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        progressWindow?.Close();
                        progressWindow = null;

                        MessageBox.Show(string.Format(SubscriberViewStrings.ExportError, e.Message),
                            SubscriberViewStrings.ExportResultTitle,
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                finally
                {
                    if (progressWindow != null)
                        await Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            progressWindow.Close();
                        });
                }
            }, CancellationToken.None);
        }


        private ITab? RemoveActiveTab()
        {
            if (ActiveTab == null)
                return null;

            var activeTabIndex = Tabs.IndexOf(ActiveTab);
            if (activeTabIndex == -1)
                return null;

            var tab = Tabs[activeTabIndex];
            Tabs.RemoveAt(activeTabIndex);

            if (activeTabIndex == Tabs.Count)
                activeTabIndex--;

            ActiveTab = activeTabIndex >= 0 ? Tabs[activeTabIndex] : null;
            closeTabCommand.RaiseCanExecuteChanged();
            undockTabCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(NoTabsVisibility));

            return tab;
        }


        private bool HasActiveTabCanExecute()
        {
            return ActiveTab != null;
        }


        public void AddTab(ITab tab)
        {
            Tabs.Add(tab);
            ActiveTab = tab;

            (tab as ITabHostWindowNotify)?.HostWindowChanged(TabHostWindow);

            closeTabCommand.RaiseCanExecuteChanged();
            undockTabCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(NoTabsVisibility));
        }


        public void ActivateTab(ITab tab)
        {
            if (undockedTabs.TryGetValue(tab, out var window))
                window.Activate();
            else if (Tabs.Contains(tab))
                ActiveTab = tab;
        }


        public void DockTab(ITab tab)
        {
            if (undockedTabs.Remove(tab, out var tabHostWindow))
                tabHostWindow.Close();

            AddTab(tab);
            ActiveTab = tab;

            (tab as ITabHostWindowNotify)?.HostWindowChanged(TabHostWindow);
        }

        public void UndockedTabClosed(ITab tab)
        {
            undockedTabs.Remove(tab);
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
            ConnectionStatusType = args.Status switch
            {
                Core.Connection.ConnectionStatus.Connected => ConnectionStatusType.Ok,
                Core.Connection.ConnectionStatus.Connecting => ConnectionStatusType.Connecting,
                _ => ConnectionStatusType.Error
            };
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
        public DesignTimeMainWindowViewModel() : base(null!, null!, null!, null!, null!, new DesignTimeTabHostProvider(), null!, null!)
        {
        }


        private class DesignTimeTabHostProvider : ITabHostProvider
        {
            public ITabHost Instance => null!;

            public void SetInstance(ITabHost instance)
            {
            }
        }
    }
}