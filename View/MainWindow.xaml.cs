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
        }
    }
}
