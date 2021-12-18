using System.Linq;
using System.Windows;

namespace PettingZoo.UI.Connection
{
    /// <summary>
    /// Interaction logic for ConnectionDisplayNameDialog.xaml
    /// </summary>
    public partial class ConnectionDisplayNameDialog
    {
        public static bool Execute(ref string displayName)
        {
            var viewModel = new ConnectionDisplayNameViewModel
            {
                DisplayName = displayName
            };


            var activeWindow = Application.Current.Windows
                .Cast<Window>()
                .FirstOrDefault(applicationWindow => applicationWindow.IsActive);

            var window = new ConnectionDisplayNameDialog(viewModel)
            {
                Owner = activeWindow ?? Application.Current.MainWindow
            };

            if (!window.ShowDialog().GetValueOrDefault())
                return false;

            displayName = viewModel.DisplayName;
            return true;
        }


        public ConnectionDisplayNameDialog(ConnectionDisplayNameViewModel viewModel)
        {
            viewModel.OkClick += (_, _) =>
            {
                DialogResult = true;
            };

            DataContext = viewModel;
            InitializeComponent();

            DisplayNameTextBox.CaretIndex = DisplayNameTextBox.Text.Length;
        }
    }
}
