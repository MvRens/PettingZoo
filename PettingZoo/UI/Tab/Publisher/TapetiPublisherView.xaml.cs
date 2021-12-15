using System.Windows;
using System.Windows.Controls;

namespace PettingZoo.UI.Tab.Publisher
{
    /// <summary>
    /// Interaction logic for TapetiPublisherView.xaml
    /// </summary>
    public partial class TapetiPublisherView
    {
        public TapetiPublisherView(TapetiPublisherViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }


        private void CaretToEnd(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            textBox.CaretIndex = textBox.Text?.Length ?? 0;
        }
    }
}
