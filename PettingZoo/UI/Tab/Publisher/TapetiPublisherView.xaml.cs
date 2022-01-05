using System.Windows;
using System.Windows.Controls;

namespace PettingZoo.UI.Tab.Publisher
{
    /// <summary>
    /// Interaction logic for TapetiPublisherView.xaml
    /// </summary>
    public partial class TapetiPublisherView
    {
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor - the XAML explicitly requires TapetiPublisherViewModel
        public TapetiPublisherView(TapetiPublisherViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            PayloadEditor.Validator = viewModel;
        }


        private void CaretToEnd(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            textBox.CaretIndex = textBox.Text.Length;
        }
    }
}
