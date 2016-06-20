using System;
using System.Windows;
using PettingZoo.ViewModel;

namespace PettingZoo.View
{
    public partial class MainWindow
    {
        public MainWindow(MainViewModel viewModel)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();
            DataContext = viewModel;

            Dispatcher.ShutdownStarted += OnDispatcherShutDownStarted;
        }


        private void OnDispatcherShutDownStarted(object sender, EventArgs e)
        {
            var disposable = DataContext as IDisposable;
            if (!ReferenceEquals(null, disposable))
                disposable.Dispose();
        }
    }
}
