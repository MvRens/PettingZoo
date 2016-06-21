using System.Windows;
using System.Windows.Input;
using PettingZoo.Model;
using PettingZoo.ViewModel;

namespace PettingZoo.View
{
    public class WindowConnectionInfoBuilder : IConnectionInfoBuilder
    {
        public ConnectionInfo Build()
        {
            var viewModel = new ConnectionViewModel(ConnectionInfo.Default());

            var dialog = new ConnectionWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            viewModel.CloseWindow += (sender, args) =>
            {
                dialog.DialogResult = true;
            };

            return dialog.ShowDialog().GetValueOrDefault() ? viewModel.ToModel() : null;
        }
    }


    public partial class ConnectionWindow
    {
        public ConnectionWindow(ConnectionViewModel viewModel)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            InitializeComponent();
            DataContext = viewModel;
        }


        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs args)
        {
            if (!char.IsDigit(args.Text, args.Text.Length - 1))
                args.Handled = true;
        }
    }
}
