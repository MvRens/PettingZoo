using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Rendering;

// TODO update title with unread message count if tab is not active
// TODO export option (to Tapeti.Cmd compatible format / command-line of course)

namespace PettingZoo.UI.Tab.Subscriber
{
    public class SubscriberViewModel : BaseViewModel, ITabToolbarCommands
    {
        private readonly ITabHost tabHost;
        private readonly ITabFactory tabFactory;
        private readonly IConnection connection;
        private readonly ISubscriber subscriber;
        private readonly TaskScheduler uiScheduler;
        private ReceivedMessageInfo? selectedMessage;
        private readonly DelegateCommand clearCommand;
        private readonly TabToolbarCommand[] toolbarCommands;
        private IDictionary<string, string>? selectedMessageProperties;

        private readonly DelegateCommand createPublisherCommand;


        public ICommand ClearCommand => clearCommand;
        public ICommand CreatePublisherCommand => createPublisherCommand;

        public ObservableCollection<ReceivedMessageInfo> Messages { get; }

        public ReceivedMessageInfo? SelectedMessage
        {
            get => selectedMessage;
            set
            {
                if (SetField(ref selectedMessage, value, otherPropertiesChanged: new[] { nameof(SelectedMessageBody) }))
                    UpdateSelectedMessageProperties();
            }
        }

        public string SelectedMessageBody =>
            SelectedMessage != null
                ? MessageBodyRenderer.Render(SelectedMessage.Body, SelectedMessage.Properties.ContentType)
                : "";

        public IDictionary<string, string>? SelectedMessageProperties
        {
            get => selectedMessageProperties;
            set => SetField(ref selectedMessageProperties, value);
        }

        public string Title => $"{subscriber.Exchange} - {subscriber.RoutingKey}";
        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        public SubscriberViewModel(ITabHost tabHost, ITabFactory tabFactory, IConnection connection, ISubscriber subscriber)
        {
            this.tabHost = tabHost;
            this.tabFactory = tabFactory;
            this.connection = connection;
            this.subscriber = subscriber;
            
            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Messages = new ObservableCollection<ReceivedMessageInfo>();
            clearCommand = new DelegateCommand(ClearExecute, ClearCanExecute);

            toolbarCommands = new[]
            {
                new TabToolbarCommand(ClearCommand, SubscriberViewStrings.CommandClear, SvgIconHelper.LoadFromResource("/Images/Clear.svg"))
            };

            createPublisherCommand = new DelegateCommand(CreatePublisherExecute, CreatePublisherCanExecute);

            subscriber.MessageReceived += SubscriberMessageReceived;
            subscriber.Start();
        }


        private void ClearExecute()
        {
            Messages.Clear();
            clearCommand.RaiseCanExecuteChanged();
        }


        private bool ClearCanExecute()
        {
            return Messages.Count > 0;
        }


        private void CreatePublisherExecute()
        {
            var publisherTab = tabFactory.CreatePublisherTab(connection, SelectedMessage);
            tabHost.AddTab(publisherTab);
        }


        private bool CreatePublisherCanExecute()
        {
            return SelectedMessage != null;
        }


        private void SubscriberMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            RunFromUiScheduler(() =>
            {
                Messages.Add(args.MessageInfo);
                clearCommand.RaiseCanExecuteChanged();
            });
        }


        private void UpdateSelectedMessageProperties()
        {
            createPublisherCommand.RaiseCanExecuteChanged();

            SelectedMessageProperties = SelectedMessage != null
                ? MessagePropertiesRenderer.Render(SelectedMessage.Properties)
                : null;
        }


        private void RunFromUiScheduler(Action action)
        {
            _ = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
        }
    }
    
    
    public class DesignTimeSubscriberViewModel : SubscriberViewModel
    {
        public DesignTimeSubscriberViewModel() : base(null!, null!, null!, new DesignTimeSubscriber())
        {
        }
        
        
        private class DesignTimeSubscriber : ISubscriber
        {
            public ValueTask DisposeAsync()
            {
                return default;
            }


            public string Exchange => "dummy";
            public string RoutingKey => "dummy";

            #pragma warning disable CS0067
            public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
            #pragma warning restore CS0067

            public void Start()
            {
            }
        }    
    }
}
