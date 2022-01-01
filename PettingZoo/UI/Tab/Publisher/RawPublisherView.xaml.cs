using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PettingZoo.UI.Tab.Publisher
{
    /// <summary>
    /// Interaction logic for RawPublisherView.xaml
    /// </summary>
    public partial class RawPublisherView
    {
        private readonly RawPublisherViewModel viewModel;
        private readonly DispatcherTimer checkEmptyHeaderTimer;


        public RawPublisherView(RawPublisherViewModel viewModel)
        {
            this.viewModel = viewModel;

            DataContext = viewModel;
            InitializeComponent();

            checkEmptyHeaderTimer = new DispatcherTimer();
            checkEmptyHeaderTimer.Tick += CheckEmptyHeaderTimerOnTick;
            checkEmptyHeaderTimer.Interval = TimeSpan.FromMilliseconds(50);
        }

        private void Header_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var dataContext = (sender as FrameworkElement)?.DataContext;
            if (dataContext is not RawPublisherViewModel.Header header)
                return;

            if (!header.IsEmpty())
                return;

            // At this point the focused element is null, so we need to check again in a bit. This will prevent
            // the header line from being removed when jumping between empty key and value textboxes
            checkEmptyHeaderTimer.Stop();
            checkEmptyHeaderTimer.Start();
        }


        private void CheckEmptyHeaderTimerOnTick(object? sender, EventArgs e)
        {
            checkEmptyHeaderTimer.Stop();

            RawPublisherViewModel.Header? focusedHeader = null;

            var focusedControl = Keyboard.FocusedElement;
            if (focusedControl is FrameworkElement { DataContext: RawPublisherViewModel.Header header })
                focusedHeader = header;

            var emptyheaders = viewModel.Headers
                .Take(viewModel.Headers.Count - 1)
                .Where(h => h != focusedHeader && h.IsEmpty())
                .ToArray();

            foreach (var emptyHeader in emptyheaders)
                viewModel.Headers.Remove(emptyHeader);
        }
    }
}
