using System.Windows;
using System.Windows.Input;

namespace PettingZoo.UI.Connection
{
    public class WindowConnectionDialog : IConnectionDialog
    {
        public ConnectionDialogParams? Show(ConnectionDialogParams? defaultParams = null)
        {
            var viewModel = new ConnectionViewModel(defaultParams ?? ConnectionDialogParams.Default);
            var window = new ConnectionWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            viewModel.OkClick += (_, _) =>
            {
                window.DialogResult = true;
            };
            
            return window.ShowDialog().GetValueOrDefault()
                ? viewModel.ToModel()
                : null;
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
