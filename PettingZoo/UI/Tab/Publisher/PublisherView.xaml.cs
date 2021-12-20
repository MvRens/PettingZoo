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
            DataContext = viewModel;
            InitializeComponent();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                Background = Brushes.Transparent;
        }
    }
}
