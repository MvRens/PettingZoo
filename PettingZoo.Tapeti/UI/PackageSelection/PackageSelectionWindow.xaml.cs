namespace PettingZoo.Tapeti.UI.PackageSelection
{
    /// <summary>
    /// Interaction logic for PackageSelectionWindow.xaml
    /// </summary>
    public partial class PackageSelectionWindow
    {
        public PackageSelectionWindow(PackageSelectionViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
