using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using PettingZoo.Core.Validation;

namespace PettingZoo.UI.Tab.Publisher
{
    /// <summary>
    /// Interaction logic for PayloadEditorControl.xaml
    /// </summary>
    public partial class PayloadEditorControl
    {
        private readonly PayloadEditorViewModel viewModel = new();


        public static readonly DependencyProperty ContentTypeProperty
            = DependencyProperty.Register(
                "ContentType",
                typeof(string),
                typeof(PayloadEditorControl),
                new FrameworkPropertyMetadata("")
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                }
            );

        public string ContentType
        {
            get => viewModel.ContentType;
            set
            {
                if (value == viewModel.ContentType)
                    return;

                SetValue(ContentTypeProperty, value);
                viewModel.ContentType = value;
            }
        }


        public static readonly DependencyProperty FixedJsonProperty
            = DependencyProperty.Register(
                "FixedJson",
                typeof(bool),
                typeof(PayloadEditorControl),
                new PropertyMetadata(false)
            );

        public bool FixedJson
        {
            get => viewModel.FixedJson;
            set
            {
                if (value == viewModel.FixedJson)
                    return;

                SetValue(FixedJsonProperty, value);
                viewModel.FixedJson = value;
            }
        }

        public static readonly DependencyProperty PayloadProperty
            = DependencyProperty.Register(
                "Payload",
                typeof(string),
                typeof(PayloadEditorControl),
                new FrameworkPropertyMetadata("")
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                }
            );

        public string Payload
        {
            get => viewModel.Payload;
            set
            {
                if (value == viewModel.Payload)
                    return;

                SetValue(PayloadProperty, value);
                viewModel.Payload = value;
            }
        }


        public IPayloadValidator? Validator
        {
            get => viewModel.Validator;
            set => viewModel.Validator = value;
        }


        private readonly ErrorHighlightingTransformer errorHighlightingTransformer = new();

        public PayloadEditorControl()
        {
            // Keep the exposed properties in sync with the ViewModel
            this.OnPropertyChanges<string>(ContentTypeProperty)
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(value =>
                {
                    viewModel.ContentType = value;
                });


            this.OnPropertyChanges<bool>(FixedJsonProperty)
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(value =>
                {
                    viewModel.FixedJson = value;
                });


            this.OnPropertyChanges<string>(PayloadProperty)
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(value =>
                {
                    viewModel.Payload = value;
                });


            viewModel.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(viewModel.ContentType):
                        SetValue(ContentTypeProperty, viewModel.ContentType);
                        break;

                    case nameof(viewModel.FixedJson):
                        SetValue(FixedJsonProperty, viewModel.FixedJson);
                        break;

                    case nameof(viewModel.Payload):
                        SetValue(PayloadProperty, viewModel.Payload);
                        break;
                }
            };

            InitializeComponent();

            // I'm not sure how to get a standard control border, all I could find were workaround:
            // https://social.msdn.microsoft.com/Forums/en-US/5e007497-8d5a-401d-ac5b-9e1356fe9b64/default-borderbrush-for-textbox-listbox-etc
            // So I'll just copy it from another TextBox. I truly hate WPF some times for making standard things so complicated. </rant>
            EditorBorder.BorderBrush = TextBoxForBorder.BorderBrush;

            Editor.Options.IndentationSize = 2;
            Editor.TextArea.TextView.LineTransformers.Add(errorHighlightingTransformer);

            // Avalon doesn't play nice with bindings it seems:
            // https://stackoverflow.com/questions/18964176/two-way-binding-to-avalonedit-document-text-using-mvvm
            Editor.Document.Text = Payload;


            var editorTriggered = false;

            Editor.TextChanged += (_, _) =>
            {
                editorTriggered = true;
                try
                {
                    Payload = Editor.Document.Text;
                }
                finally
                {
                    editorTriggered = false;
                }
            };


            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != nameof(viewModel.ValidationInfo) && 
                    (args.PropertyName != nameof(viewModel.Payload) || editorTriggered))
                    return;

                Dispatcher.Invoke(() =>
                {
                    switch (args.PropertyName)
                    {
                        case nameof(viewModel.ValidationInfo):
                            if (errorHighlightingTransformer.ErrorPosition == viewModel.ValidationInfo.ErrorPosition)
                                return;

                            errorHighlightingTransformer.ErrorPosition = viewModel.ValidationInfo.ErrorPosition;

                            // This can probably be optimized to only redraw the affected line, but the message is typically so small it's not worth the effort at the moment
                            Editor.TextArea.TextView.Redraw();
                            break;

                        case nameof(viewModel.Payload):
                            Editor.Document.Text = viewModel.Payload;
                            break;
                    }
                });
            };


            // Setting the DataContext for the UserControl is a major PITA when binding the control's properties,
            // so I've moved the ViewModel one level down to get the best of both worlds...
            DataContextContainer.DataContext = viewModel;
        }
    }
}
