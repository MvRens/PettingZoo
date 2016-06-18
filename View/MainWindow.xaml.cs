using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using PettingZoo.ViewModel;

namespace PettingZoo.View
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
