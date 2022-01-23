using System.Windows;
using System.Windows.Controls;

namespace PettingZoo.WPF.Controls
{
    public class NoOverflowToolbar : ToolBar
    {
        public NoOverflowToolbar()
        {
            Loaded += (_, _) =>
            {
                // Hide arrow on the right side of the toolbar
                if (Template.FindName("OverflowGrid", this) is FrameworkElement overflowGrid)
                    overflowGrid.Visibility = Visibility.Collapsed;

                if (Template.FindName("MainPanelBorder", this) is FrameworkElement mainPanelBorder)
                    mainPanelBorder.Margin = new Thickness(0);
            };
        }
    }
}
