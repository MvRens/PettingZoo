using System.Windows;
using System.Windows.Input;
using AutoMapper;
using PettingZoo.Connection;
using PettingZoo.Infrastructure;
using PettingZoo.ViewModel;

namespace PettingZoo.View
{
    public class WindowConnectionInfoBuilder : IConnectionInfoBuilder
    {
        private readonly UserSettings userSettings;

        private static readonly IMapper ConnectionInfoMapper = new MapperConfiguration(cfg =>
        {
            cfg.RecognizeDestinationPrefixes("Last");
            cfg.RecognizePrefixes("Last");

            cfg.CreateMap<ConnectionInfo, ConnectionWindowSettings>().ReverseMap();
        }).CreateMapper();


        public WindowConnectionInfoBuilder(UserSettings userSettings)
        {
            this.userSettings = userSettings;
        }


        public ConnectionInfo Build()
        {
            var connectionInfo = ConnectionInfoMapper.Map<ConnectionInfo>(userSettings.ConnectionWindow);
            var viewModel = new ConnectionViewModel(connectionInfo);

            var dialog = new ConnectionWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            viewModel.CloseWindow += (sender, args) =>
            {
                dialog.DialogResult = true;
            };

            connectionInfo = dialog.ShowDialog().GetValueOrDefault() ? viewModel.ToModel() : null;
            if (connectionInfo != null)
            {
                ConnectionInfoMapper.Map(connectionInfo, userSettings.ConnectionWindow);
                userSettings.Save();
            }

            return connectionInfo;
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
