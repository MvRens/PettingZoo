using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Rendering;
using PettingZoo.WPF.ViewModel;

// TODO if the "New message" line is visible when this tab is undocked, the line in the ListBox does not shrink. Haven't been able to figure out yet how to solve it

namespace PettingZoo.UI.Tab.Subscriber
{
    public class SubscriberViewModel : BaseViewModel, ITabToolbarCommands, ITabActivate
    {
        private readonly ITabHost tabHost;
        private readonly ITabFactory tabFactory;
        private readonly IConnection connection;
        private readonly ISubscriber subscriber;
        private readonly Dispatcher dispatcher;
        private ReceivedMessageInfo? selectedMessage;
        private readonly DelegateCommand clearCommand;
        private readonly TabToolbarCommand[] toolbarCommands;
        private IDictionary<string, string>? selectedMessageProperties;

        private readonly DelegateCommand createPublisherCommand;

        private bool tabActive;
        private ReceivedMessageInfo? newMessage;
        private Timer? newMessageTimer;
        private int unreadCount;


        public ICommand ClearCommand => clearCommand;

        // ReSharper disable once UnusedMember.Global - it is, but via a proxy
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


        public ReceivedMessageInfo? NewMessage
        {
            get => newMessage;
            set => SetField(ref newMessage, value);
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

        public string Title => 
            (subscriber.Exchange != null ? $"{subscriber.Exchange} - {subscriber.RoutingKey}" : $"{subscriber.QueueName}") +
            (tabActive || unreadCount == 0 ? "" : $" ({unreadCount})");
        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        public SubscriberViewModel(ITabHost tabHost, ITabFactory tabFactory, IConnection connection, ISubscriber subscriber)
        {
            this.tabHost = tabHost;
            this.tabFactory = tabFactory;
            this.connection = connection;
            this.subscriber = subscriber;

            dispatcher = Dispatcher.CurrentDispatcher;

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
            dispatcher.BeginInvoke(() =>
            {
                if (!tabActive)
                {
                    unreadCount++;
                    RaisePropertyChanged(nameof(Title));

                    NewMessage ??= args.MessageInfo;
                }

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


        public void Activate()
        {
            tabActive = true;
            unreadCount = 0;

            RaisePropertyChanged(nameof(Title));

            if (NewMessage == null) 
                return;

            newMessageTimer?.Dispose();
            newMessageTimer = new Timer(
                _ =>
                {
                    dispatcher.BeginInvoke(() =>
                    {
                        NewMessage = null;
                    });
                },
                null,
                TimeSpan.FromSeconds(5),
                Timeout.InfiniteTimeSpan);
        }

        public void Deactivate()
        {
            if (newMessageTimer != null)
            {
                newMessageTimer.Dispose();
                newMessageTimer = null;
            }

            NewMessage = null;
            tabActive = false;
        }
    }
    
    
    public class DesignTimeSubscriberViewModel : SubscriberViewModel
    {
        public DesignTimeSubscriberViewModel() : base(null!, null!, null!, new DesignTimeSubscriber())
        {
            for (var i = 1; i <= 5; i++)
                Messages.Add(new ReceivedMessageInfo(
                    "designtime", 
                    $"designtime.message.{i}", 
                    Encoding.UTF8.GetBytes(@"Design-time message"), 
                    new MessageProperties(null)
                    {
                        ContentType = "text/fake",
                        ReplyTo = "/dev/null"
                    }, 
                    DateTime.Now));

            SelectedMessage = Messages[2];
            NewMessage = Messages[2];
        }
        
        
        private class DesignTimeSubscriber : ISubscriber
        {
            public ValueTask DisposeAsync()
            {
                return default;
            }


            public string QueueName => "dummy";
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
