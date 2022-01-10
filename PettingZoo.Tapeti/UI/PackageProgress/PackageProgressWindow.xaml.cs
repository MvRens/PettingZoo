using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PettingZoo.Tapeti.UI.PackageProgress
{
    /// <summary>
    /// Interaction logic for PackageProgressWindow.xaml
    /// </summary>
    public partial class PackageProgressWindow : IProgress<int>
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public CancellationToken CancellationToken => cancellationTokenSource.Token;


        public PackageProgressWindow()
        {
            InitializeComponent();

            Closed += (_, _) =>
            {
                cancellationTokenSource.Cancel();
            };
        }


        public void Report(int value)
        {
            Dispatcher.BeginInvoke(() =>
            {
                Progress.Value = value;
            });
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            ((Button)sender).IsEnabled = false;
        }
    }
}
