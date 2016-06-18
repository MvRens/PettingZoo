using System.Windows;
using PettingZoo.Model;
using PettingZoo.ViewModel;

namespace PettingZoo.View
{
    public class WindowConnectionInfoBuilder : IConnectionInfoBuilder
    {
        public ConnectionInfo Build()
        {
            var viewModel = new ConnectionWindowViewModel();
            var dialog = new ConnectionWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            if (!dialog.ShowDialog().GetValueOrDefault())
                return null;

            return viewModel.ConnectionInfo;
        }
    }


    public partial class ConnectionWindow : Window
    {
        public ConnectionWindow(ConnectionWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
