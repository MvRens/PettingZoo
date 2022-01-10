using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Rendering;
using PettingZoo.WPF.ViewModel;

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
        private Timer? newMessageTimer;
        private int unreadCount;


        public ICommand ClearCommand => clearCommand;

        // ReSharper disable once UnusedMember.Global - it is, but via a proxy
        public ICommand CreatePublisherCommand => createPublisherCommand;

        public ObservableCollectionEx<ReceivedMessageInfo> Messages { get; }
        public ObservableCollectionEx<ReceivedMessageInfo> UnreadMessages { get; }

        public ReceivedMessageInfo? SelectedMessage
        {
            get => selectedMessage;
            set
            {
                if (SetField(ref selectedMessage, value, otherPropertiesChanged: new[] { nameof(SelectedMessageBody) }))
                    UpdateSelectedMessageProperties();
            }
        }


        public Visibility UnreadMessagesVisibility => UnreadMessages.Count > 0 ? Visibility.Visible : Visibility.Collapsed;


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

            Messages = new ObservableCollectionEx<ReceivedMessageInfo>();
            UnreadMessages = new ObservableCollectionEx<ReceivedMessageInfo>();
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
            UnreadMessages.Clear();
            RaisePropertyChanged(nameof(UnreadMessagesVisibility));
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

                    UnreadMessages.Add(args.MessageInfo);
                    if (UnreadMessages.Count == 1)
                        RaisePropertyChanged(nameof(UnreadMessagesVisibility));
                }
                else
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

            if (UnreadMessages.Count == 0) 
                return;

            newMessageTimer?.Dispose();
            newMessageTimer = new Timer(
                _ =>
                {
                    dispatcher.BeginInvoke(() =>
                    {
                        if (UnreadMessages.Count == 0)
                            return;

                        Messages.BeginUpdate();
                        UnreadMessages.BeginUpdate();
                        try
                        {
                            Messages.AddRange(UnreadMessages);
                            UnreadMessages.Clear();
                        }
                        finally
                        {
                            UnreadMessages.EndUpdate();
                            Messages.EndUpdate();
                        }

                        RaisePropertyChanged(nameof(UnreadMessagesVisibility));
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

            tabActive = false;
        }
    }
    
    
    public class DesignTimeSubscriberViewModel : SubscriberViewModel
    {
        public DesignTimeSubscriberViewModel() : base(null!, null!, null!, new DesignTimeSubscriber())
        {
            for (var i = 1; i <= 5; i++)
                (i > 2 ? UnreadMessages : Messages).Add(new ReceivedMessageInfo(
                    "designtime",
                    $"designtime.message.{i}",
                    Encoding.UTF8.GetBytes(@"Design-time message"),
                    new MessageProperties(null)
                    {
                        ContentType = "text/fake",
                        ReplyTo = "/dev/null"
                    },
                    DateTime.Now));

            SelectedMessage = UnreadMessages[0];
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
