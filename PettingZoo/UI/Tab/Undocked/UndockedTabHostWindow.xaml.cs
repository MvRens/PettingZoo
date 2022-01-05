using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PettingZoo.UI.Tab.Undocked
{
    /// <summary>
    /// Interaction logic for UndockedTabHostWindow.xaml
    /// </summary>
    public partial class UndockedTabHostWindow
    {
        public static UndockedTabHostWindow Create(ITabHost tabHost, ITab tab, double width, double height)
        {
            var viewModel = new UndockedTabHostViewModel(tabHost, tab);
            var window = new UndockedTabHostWindow(viewModel)
            {
                Width = width,
                Height = height
            };

            return window;
        }


        public UndockedTabHostWindow(UndockedTabHostViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            Closed += (_, _) =>
            {
                viewModel.WindowClosed();
            };

            Activated += (_, _) =>
            {
                viewModel.Activate();
            };

            Deactivated += (_, _) =>
            {
                viewModel.Deactivate();
            };
        }

        private void Toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide arrow on the right side of the toolbar
            var toolBar = sender as ToolBar;

            if (toolBar?.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
                overflowGrid.Visibility = Visibility.Collapsed;

            if (toolBar?.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
                mainPanelBorder.Margin = new Thickness(0);
        }
    }
}
