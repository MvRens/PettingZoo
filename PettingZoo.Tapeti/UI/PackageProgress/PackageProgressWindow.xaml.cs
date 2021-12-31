using System;

namespace PettingZoo.Tapeti.UI.PackageProgress
{
    /// <summary>
    /// Interaction logic for PackageProgressWindow.xaml
    /// </summary>
    public partial class PackageProgressWindow : IProgress<int>
    {
        public PackageProgressWindow()
        {
            InitializeComponent();
        }


        public void Report(int value)
        {
            Dispatcher.BeginInvoke(() =>
            {
                Progress.Value = value;
            });
        }
    }
}
