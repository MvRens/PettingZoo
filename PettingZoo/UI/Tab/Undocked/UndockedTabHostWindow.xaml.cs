using System.Windows;

namespace PettingZoo.UI.Tab.Undocked
{
    /// <summary>
    /// Interaction logic for UndockedTabHostWindow.xaml
    /// </summary>
    public partial class UndockedTabHostWindow
    {
        public static UndockedTabHostWindow Create(ITabHost tabHost, ITab tab, double width, double height)
        {
            var viewModel = new UndockedTabHostViewModel(tabHost, tab);
            var window = new UndockedTabHostWindow(viewModel)
            {
                Width = width,
                Height = height
            };

            return window;
        }


        public UndockedTabHostWindow(UndockedTabHostViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            Closed += (_, _) =>
            {
                viewModel.WindowClosed();
            };
        }
    }
}
