using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PettingZoo.Core.Settings;

namespace PettingZoo.UI.Connection
{
    public class WindowConnectionDialog : IConnectionDialog
    {
        private readonly IConnectionSettingsRepository connectionSettingsRepository;

        public WindowConnectionDialog(IConnectionSettingsRepository connectionSettingsRepository)
        {
            this.connectionSettingsRepository = connectionSettingsRepository;
        }


        public async Task<ConnectionSettings?> Show()
        {
            var lastUsed = await connectionSettingsRepository.GetLastUsed();

            var viewModel = new ConnectionViewModel(connectionSettingsRepository, lastUsed);
            await viewModel.Initialize();

            var window = new ConnectionWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            viewModel.OkClick += (_, _) =>
            {
                window.DialogResult = true;
            };

            
            if (!window.ShowDialog().GetValueOrDefault())
                return null;

            var newSettings = viewModel.ToModel();
            await connectionSettingsRepository.StoreLastUsed(newSettings);

            return newSettings;
        }
    }


    public partial class ConnectionWindow
    {
        public ConnectionWindow(ConnectionViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }


        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs args)
        {
            if (!char.IsDigit(args.Text, args.Text.Length - 1))
                args.Handled = true;
        }


        private void CaretToEnd(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            textBox.CaretIndex = textBox.Text.Length;
        }
    }
}
