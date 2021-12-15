using System;
using System.Windows;
using PettingZoo.Core.Connection;
using PettingZoo.UI.Connection;
using PettingZoo.UI.Subscribe;
using PettingZoo.UI.Tab;

namespace PettingZoo.UI.Main
{
    // TODO support undocking tabs (and redocking afterwards)
    // TODO allow tab reordering

    #pragma warning disable CA1001 // MainWindow can't be IDisposable, handled instead in OnDispatcherShutDownStarted
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;
        

        public MainWindow(IConnectionFactory connectionFactory, IConnectionDialog connectionDialog, ISubscribeDialog subscribeDialog)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();
            viewModel = new MainWindowViewModel(connectionFactory, connectionDialog, subscribeDialog);
            DataContext = viewModel;

            Dispatcher.ShutdownStarted += OnDispatcherShutDownStarted;
        }


        private async void OnDispatcherShutDownStarted(object? sender, EventArgs e)
        {
            if (DataContext is IAsyncDisposable disposable)
                await disposable.DisposeAsync();
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            viewModel.ConnectCommand.Execute(null);
        }

        
        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            var _ = Application.Current.Windows;
        }
    }
    #pragma warning restore CA1001
}
