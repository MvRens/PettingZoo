using System.Windows;
using System.Windows.Controls;

namespace PettingZoo.WPF.Controls
{
    public class AutoOverflowToolBar : ToolBar
    {
        public AutoOverflowToolBar()
        {
            Loaded += (_, _) =>
            {
                // Hide overflow arrow on the right side of the toolbar when not required
                if (Template.FindName("OverflowButton", this) is not FrameworkElement overflowButton)
                    return;

                if (!overflowButton.IsEnabled)
                    overflowButton.Visibility = Visibility.Hidden;

                overflowButton.IsEnabledChanged += (_, _) =>
                {
                    overflowButton.Visibility = overflowButton.IsEnabled ? Visibility.Visible : Visibility.Hidden;
                };
            };
        }
    }
}
