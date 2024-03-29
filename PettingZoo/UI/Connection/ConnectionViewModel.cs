﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PettingZoo.Core.Settings;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Connection
{
    public class ConnectionViewModel : BaseViewModel
    {
        private readonly IConnectionSettingsRepository connectionSettingsRepository;
        private readonly StoredConnectionSettings defaultSettings;
        private string host = null!;
        private string virtualHost = null!;
        private int port;
        private string username = null!;
        private string password = null!;

        private bool subscribe;
        private string exchange = null!;
        private string routingKey = null!;

        private bool storePassword;

        private StoredConnectionSettings? selectedStoredConnection;

        private readonly DelegateCommand okCommand;
        private readonly DelegateCommand saveCommand;
        private readonly DelegateCommand saveAsCommand;
        private readonly DelegateCommand deleteCommand;

        private readonly DelegateCommand[] connectionChangedCommands;


        public string Host
        {
            get => host;
            set => SetField(ref host, value, delegateCommandsChanged: connectionChangedCommands);
        }

        public string VirtualHost
        {
            get => virtualHost;
            set => SetField(ref virtualHost, value, delegateCommandsChanged: connectionChangedCommands);
        }

        public int Port
        {
            get => port;
            set => SetField(ref port, value, delegateCommandsChanged: connectionChangedCommands);
        }

        public string Username
        {
            get => username;
            set => SetField(ref username, value, delegateCommandsChanged: connectionChangedCommands);
        }

        public string Password
        {
            get => password;
            set => SetField(ref password, value, delegateCommandsChanged: connectionChangedCommands);
        }


        public bool Subscribe
        {
            get => subscribe;
            set => SetField(ref subscribe, value, delegateCommandsChanged: connectionChangedCommands);
        }

        public string Exchange
        {
            get => exchange;
            set
            {
                if (SetField(ref exchange, value, delegateCommandsChanged: connectionChangedCommands))
                    AutoToggleSubscribe();
            }
        }

        public string RoutingKey
        {
            get => routingKey;
            set
            {
                if (SetField(ref routingKey, value, delegateCommandsChanged: connectionChangedCommands))
                    AutoToggleSubscribe();
            }
        }


        public bool StorePassword
        {
            get => storePassword;
            set => SetField(ref storePassword, value, delegateCommandsChanged: connectionChangedCommands);
        }


        public ObservableCollection<StoredConnectionSettings> StoredConnections { get; } = new();

        public StoredConnectionSettings? SelectedStoredConnection
        {
            get => selectedStoredConnection;
            set
            {
                if (value == null)
                    return;

                if (!SetField(ref selectedStoredConnection, value, delegateCommandsChanged: new [] { deleteCommand }))
                    return;

                DisableCommandsChanged(
                    () =>
                    {
                        Host = value.Host;
                        VirtualHost = value.VirtualHost;
                        Port = value.Port;
                        Username = value.Username;
                        Password = value.Password;
                        StorePassword = value.StorePassword;

                        Exchange = value.Exchange;
                        RoutingKey = value.RoutingKey;
                        Subscribe = value.Subscribe;
                    },
                    connectionChangedCommands);
            }
        }


        public ICommand OkCommand => okCommand;
        public ICommand SaveCommand => saveCommand;
        public ICommand SaveAsCommand => saveAsCommand;
        public ICommand DeleteCommand => deleteCommand;

        public event EventHandler? OkClick;


        public ConnectionViewModel(IConnectionSettingsRepository connectionSettingsRepository, StoredConnectionSettings defaultSettings)
        {
            this.connectionSettingsRepository = connectionSettingsRepository;
            this.defaultSettings = defaultSettings;

            okCommand = new DelegateCommand(OkExecute, OkCanExecute);
            saveCommand = new DelegateCommand(SaveExecute, SaveCanExecute);
            saveAsCommand = new DelegateCommand(SaveAsExecute, SaveAsCanExecute);
            deleteCommand = new DelegateCommand(DeleteExecute, DeleteCanExecute);

            connectionChangedCommands = new[] { saveCommand, saveAsCommand, okCommand };
        }


        public async Task Initialize()
        {
            var defaultConnection = new StoredConnectionSettings(
                Guid.Empty,
                ConnectionWindowStrings.LastUsedDisplayName,
                defaultSettings.StorePassword,
                defaultSettings.Host,
                defaultSettings.VirtualHost,
                defaultSettings.Port,
                defaultSettings.Username,
                defaultSettings.Password,
                defaultSettings.Subscribe,
                defaultSettings.Exchange,
                defaultSettings.RoutingKey);

            var isStored = false;

            foreach (var storedConnectionSettings in await connectionSettingsRepository.GetStored())
            {
                if (!isStored && storedConnectionSettings.SameParameters(defaultConnection, false))
                {
                    SelectedStoredConnection = storedConnectionSettings;
                    isStored = true;
                }

                StoredConnections.Add(storedConnectionSettings);
            }

            if (isStored)
            {
                // The last used parameters match a stored connection, insert the "New connection" item with default parameters
                StoredConnections.Insert(0, new StoredConnectionSettings(Guid.Empty, ConnectionWindowStrings.LastUsedDisplayName, true, ConnectionSettings.Default));
            }
            else
            {
                // No match, use the passed parameters
                StoredConnections.Insert(0, defaultConnection);
                SelectedStoredConnection = defaultConnection;
            }
        }


        public ConnectionSettings ToModel()
        {
            return new ConnectionSettings(Host, VirtualHost, Port, Username, Password, Subscribe, Exchange, RoutingKey);
        }


        private bool ValidConnection(bool requirePassword)
        {
            return !string.IsNullOrWhiteSpace(Host) &&
                   !string.IsNullOrWhiteSpace(VirtualHost) &&
                   Port > 0 &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   (!requirePassword || !string.IsNullOrWhiteSpace(Password)) &&
                   (!Subscribe || !string.IsNullOrWhiteSpace(Exchange) && !string.IsNullOrWhiteSpace(RoutingKey));
        }


        private void AutoToggleSubscribe()
        {
            Subscribe = !string.IsNullOrWhiteSpace(Exchange) && !string.IsNullOrWhiteSpace(RoutingKey);
        }


        private void OkExecute()
        {
            OkClick?.Invoke(this, EventArgs.Empty);
        }


        private bool OkCanExecute()
        {
            return ValidConnection(true);
        }


        private async void SaveExecute()
        {
            if (SelectedStoredConnection == null || SelectedStoredConnection.Id == Guid.Empty)
                return;

            var selectedIndex = StoredConnections.IndexOf(SelectedStoredConnection);

            var updatedStoredConnection = await connectionSettingsRepository.Update(SelectedStoredConnection.Id, SelectedStoredConnection.DisplayName, StorePassword, ToModel());


            StoredConnections[selectedIndex] = updatedStoredConnection;
            SelectedStoredConnection = updatedStoredConnection;
        }


        private bool SaveCanExecute()
        {
            return SelectedStoredConnection != null && 
                   SelectedStoredConnection.Id != Guid.Empty &&
                   ValidConnection(false) &&
                   (
                       !ToModel().SameParameters(SelectedStoredConnection, StorePassword) ||
                       SelectedStoredConnection.StorePassword != StorePassword
                   );
        }


        private async void SaveAsExecute()
        {
            // TODO create and enforce unique name?
            var displayName = SelectedStoredConnection != null && SelectedStoredConnection.Id != Guid.Empty ? SelectedStoredConnection.DisplayName : "";

            if (!InputDialog.Execute(ref displayName, ConnectionWindowStrings.ProfileNameDialogTitle))
                return;

            var storedConnectionSettings = await connectionSettingsRepository.Add(displayName, StorePassword, ToModel());

            StoredConnections.Add(storedConnectionSettings);
            SelectedStoredConnection = storedConnectionSettings;
        }


        private bool SaveAsCanExecute()
        {
            return ValidConnection(false);
        }


        private async void DeleteExecute()
        {
            if (SelectedStoredConnection == null || SelectedStoredConnection.Id == Guid.Empty)
                return;

            var selectedIndex = StoredConnections.IndexOf(SelectedStoredConnection);

            if (MessageBox.Show(
                    string.Format(ConnectionWindowStrings.DeleteConfirm, SelectedStoredConnection.DisplayName), 
                    ConnectionWindowStrings.DeleteConfirmTitle, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            await connectionSettingsRepository.Delete(SelectedStoredConnection.Id);

            StoredConnections.Remove(SelectedStoredConnection);
            if (selectedIndex >= StoredConnections.Count)
                selectedIndex--;

            SelectedStoredConnection = StoredConnections[selectedIndex];
        }


        private bool DeleteCanExecute()
        {
            return SelectedStoredConnection != null && SelectedStoredConnection.Id != Guid.Empty;
        }
    }


    public class DesignTimeConnectionViewModel : ConnectionViewModel
    {
        public DesignTimeConnectionViewModel() : base(null!, null!)
        {
            StoredConnections.Add(new StoredConnectionSettings(Guid.Empty, "Dummy", true, ConnectionSettings.Default));
        }
    }
}
