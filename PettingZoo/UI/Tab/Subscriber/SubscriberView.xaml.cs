using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PettingZoo.UI.Tab.Subscriber
{
    /// <summary>
    /// Interaction logic for SubscriberView.xaml
    /// </summary>
    public partial class SubscriberView
    {
        public SubscriberView(SubscriberViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();


            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                Background = Brushes.Transparent;
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
