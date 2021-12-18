using System.Collections.Generic;
using System.Windows.Media;

namespace PettingZoo.UI.Tab.Subscriber
{
    /// <summary>
    /// Interaction logic for SubscriberView.xaml
    /// </summary>
    public partial class SubscriberView
    {
        public SubscriberView(SubscriberViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();


            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                Background = Brushes.Transparent;
        }
    }
}
