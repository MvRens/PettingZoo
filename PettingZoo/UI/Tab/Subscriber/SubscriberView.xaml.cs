using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;

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

            // TODO wrap all this nonsense (which is a duplicate from PayloadEditorControl) in a UserControl
            // should contain the border, one- or two-way Document binding and the automatic syntax highlighting based on a bound content-type
            EditorBorder.BorderBrush = ReferenceControlForBorder.BorderBrush;
            Editor.Options.IndentationSize = 2;

            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != nameof(viewModel.SelectedMessageBody))
                    return;

                Editor.Document.Text = viewModel.SelectedMessageBody;
                Editor.SyntaxHighlighting = (viewModel.SelectedMessage?.Properties.ContentType ?? "") == "application/json"
                        ? HighlightingManager.Instance.GetDefinition(@"Json")
                        : null;
            };


            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                Background = Brushes.Transparent;
        }
    }
}
