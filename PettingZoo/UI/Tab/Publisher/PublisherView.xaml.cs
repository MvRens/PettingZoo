using System.Windows.Media;

namespace PettingZoo.UI.Tab.Publisher
{
    /// <summary>
    /// Interaction logic for PublisherView.xaml
    /// </summary>
    public partial class PublisherView
    {
        public PublisherView(PublisherViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                Background = Brushes.Transparent;
        }
    }
}
