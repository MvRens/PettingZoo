using System.Windows;

namespace PettingZoo.Tapeti.UI.ClassSelection
{
    /// <summary>
    /// Interaction logic for ClassSelectionWindow.xaml
    /// </summary>
    public partial class ClassSelectionWindow
    {
        private readonly ClassSelectionViewModel viewModel;


        public ClassSelectionWindow(ClassSelectionViewModel viewModel)
        {
            this.viewModel = viewModel;

            DataContext = viewModel;
            InitializeComponent();
        }


        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            viewModel.SelectedItem = (BaseClassTreeItem)e.NewValue;
        }
    }
}
