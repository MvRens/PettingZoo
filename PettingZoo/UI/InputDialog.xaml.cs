using System.Linq;
using System.Windows;

namespace PettingZoo.UI
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog
    {
        public static bool Execute(ref string value, string title)
        {
            var viewModel = new InputDialogViewModel
            {
                Value = value,
                Title = title
            };


            var activeWindow = Application.Current.Windows
                .Cast<Window>()
                .FirstOrDefault(applicationWindow => applicationWindow.IsActive);

            var window = new InputDialog(viewModel)
            {
                Owner = activeWindow ?? Application.Current.MainWindow
            };

            if (!window.ShowDialog().GetValueOrDefault())
                return false;

            value = viewModel.Value;
            return true;
        }


        public InputDialog(InputDialogViewModel viewModel)
        {
            viewModel.OkClick += (_, _) =>
            {
                DialogResult = true;
            };

            DataContext = viewModel;
            InitializeComponent();

            ValueTextBox.CaretIndex = ValueTextBox.Text.Length;
        }
    }
}
