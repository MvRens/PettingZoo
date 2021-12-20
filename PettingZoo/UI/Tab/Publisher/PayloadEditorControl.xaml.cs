using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;

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

            // Setting the DataContext for the UserControl is a major PITA when binding the control's properties,
            // so I've moved the ViewModel one level down to get the best of both worlds...
            DataContextContainer.DataContext = viewModel;
        }
    }
}
