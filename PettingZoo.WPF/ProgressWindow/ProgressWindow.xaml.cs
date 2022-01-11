using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PettingZoo.WPF.ProgressWindow
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : IProgress<int>
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public CancellationToken CancellationToken => cancellationTokenSource.Token;


        public ProgressWindow(string title)
        {
            InitializeComponent();
            Title = title;

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
