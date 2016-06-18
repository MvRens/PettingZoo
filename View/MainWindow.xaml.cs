using System.Windows;
using PettingZoo.ViewModel;

namespace PettingZoo.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainBaseViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
