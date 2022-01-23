using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PettingZoo.Core.Connection;
using PettingZoo.Core.ExportImport.Subscriber;
using PettingZoo.UI.Connection;
using PettingZoo.UI.Subscribe;
using PettingZoo.UI.Tab;
using Serilog;

namespace PettingZoo.UI.Main
{
    #pragma warning disable CA1001 // MainWindow can't be IDisposable, handled instead in OnDispatcherShutDownStarted
    public partial class MainWindow : ITabContainer
    {
        private readonly MainWindowViewModel viewModel;

        public bool WasMaximized;
        

        public MainWindow(ILogger logger, IConnectionFactory connectionFactory, IConnectionDialog connectionDialog, ISubscribeDialog subscribeDialog,
            ITabHostProvider tabHostProvider, ITabFactory tabFactory, IExportImportFormatProvider exportImportFormatProvider)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            viewModel = new MainWindowViewModel(logger, connectionFactory, connectionDialog, subscribeDialog, this, tabHostProvider, tabFactory, exportImportFormatProvider)
            {
                TabHostWindow = this
            };

            DataContext = viewModel;
            InitializeComponent();

            Dispatcher.ShutdownStarted += OnDispatcherShutDownStarted;


            // If the WindowState is Minimized, we can't tell if it was maximized before. To properly store
            // the last window position, keep track of it.
            this.OnPropertyChanges<WindowState>(WindowStateProperty)
                .Subscribe(newState =>
                {
                    WasMaximized = newState switch
                    {
                        WindowState.Maximized => true,
                        WindowState.Normal => false,
                        _ => WasMaximized
                    };
                });
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


        private void TabItem_PreviewRightMouseDown(object sender, MouseButtonEventArgs e)
        {
            var tabItem = GetParent<TabItem>(e.OriginalSource);
            if (tabItem == null)
                return;

            var tabControl = GetParent<TabControl>(tabItem);
            if (tabControl == null)
                return;

            tabControl.SelectedItem = tabItem.DataContext;
        }


        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Source is not TabItem tabItem)
                return;

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
        }


        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            var targetTab = GetParent<TabItem>(e.OriginalSource);
            if (targetTab == null)
                return;

            var sourceTab = (TabItem?)e.Data.GetData(typeof(TabItem));
            if (sourceTab == null || sourceTab == targetTab)
                return;

            var tabControl = GetParent<TabControl>(targetTab);
            if (tabControl?.ItemsSource is not ObservableCollection<ITab> dataCollection)
                return;

            if (sourceTab.DataContext is not ITab sourceData || targetTab.DataContext is not ITab targetData)
                return;

            var sourceIndex = dataCollection.IndexOf(sourceData);
            var targetIndex = dataCollection.IndexOf(targetData);

            dataCollection.Move(sourceIndex, targetIndex);
        }


        private static T? GetParent<T>(object originalSource) where T : DependencyObject
        {
            var current = originalSource as DependencyObject;
            if (current is not Visual)
                return null;

            while (current != null)
            {
                if (current is T targetType)
                    return targetType;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }


        public double TabWidth => SubscriberTabs.ActualWidth;
        public double TabHeight => SubscriberTabs.ActualHeight;
    }
    #pragma warning restore CA1001
}
