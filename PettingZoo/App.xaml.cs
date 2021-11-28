using System.Windows;
using System.Windows.Threading;

namespace PettingZoo
{
    public partial class App
    {
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _ = MessageBox.Show($"Unhandled exception: {e.Exception.Message}", "Petting Zoo - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
