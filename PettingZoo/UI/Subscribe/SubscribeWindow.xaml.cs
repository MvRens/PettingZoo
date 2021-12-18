using System.Windows;

namespace PettingZoo.UI.Subscribe
{
    public class WindowSubscribeDialog : ISubscribeDialog
    {
        public SubscribeDialogParams? Show(SubscribeDialogParams? defaultParams = null)
        {
            var viewModel = new SubscribeViewModel(defaultParams ?? SubscribeDialogParams.Default);
            var window = new SubscribeWindow(viewModel)
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


    public partial class SubscribeWindow
    {
        public SubscribeWindow(SubscribeViewModel viewModel)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
