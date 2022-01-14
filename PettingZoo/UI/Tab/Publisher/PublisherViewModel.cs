using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Generator;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Tab.Publisher
{
    public enum MessageType
    {
        Raw,
        Tapeti
    }
    

    public class PublisherViewModel : BaseViewModel, ITabToolbarCommands, ITabHostWindowNotify, IPublishDestination
    {
        private readonly IConnection connection;
        private readonly IExampleGenerator exampleGenerator;
        private readonly ITabFactory tabFactory;

        private bool sendToExchange = true;
        private string exchange = "";
        private string routingKey = "";
        private string queue = "";
        private string replyTo = "";
        private bool replyToSpecified = true;

        private MessageType messageType;
        private UserControl? messageTypeControl;
        private ICommand? messageTypePublishCommand;

        private UserControl? rawPublisherView;
        private UserControl? tapetiPublisherView;

        private readonly DelegateCommand publishCommand;
        private readonly TabToolbarCommand[] toolbarCommands;
        private Window? tabHostWindow;


        public bool SendToExchange
        {
            get => sendToExchange;
            set => SetField(ref sendToExchange, value, 
                delegateCommandsChanged: new [] { publishCommand },
                otherPropertiesChanged: new[] { nameof(SendToQueue), nameof(ExchangeVisibility), nameof(QueueVisibility), nameof(Title) });
        }


        public bool SendToQueue
        {
            get => !SendToExchange;
            set => SendToExchange = !value;
        }


        public string Exchange
        {
            get => exchange;
            set => SetField(ref exchange, value, delegateCommandsChanged: new[] { publishCommand });
        }


        public string RoutingKey
        {
            get => routingKey;
            set => SetField(ref routingKey, value, delegateCommandsChanged: new[] { publishCommand }, otherPropertiesChanged: new[] { nameof(Title) });
        }


        public string Queue
        {
            get => queue;
            set => SetField(ref queue, value, delegateCommandsChanged: new[] { publishCommand }, otherPropertiesChanged: new[] { nameof(Title) });
        }


        public string ReplyTo
        {
            get => replyTo;
            set => SetField(ref replyTo, value);
        }


        public bool ReplyToSpecified
        {
            get => replyToSpecified;
            set => SetField(ref replyToSpecified, value, otherPropertiesChanged: new[] { nameof(ReplyToNewSubscriber) });
        }


        public bool ReplyToNewSubscriber
        {
            get => !ReplyToSpecified;
            set => ReplyToSpecified = !value;
        }


        public virtual Visibility ExchangeVisibility => SendToExchange ? Visibility.Visible : Visibility.Collapsed;
        public virtual Visibility QueueVisibility => SendToQueue ? Visibility.Visible : Visibility.Collapsed;


        public MessageType MessageType
        {
            get => messageType;
            set
            {
                if (SetField(ref messageType, value,
                    otherPropertiesChanged: new[]
                    {
                        nameof(MessageTypeRaw),
                        nameof(MessageTypeTapeti)
                    }))
                {
                    SetMessageTypeControl(value);
                }
            }
        }

        public bool MessageTypeRaw
        {
            get => MessageType == MessageType.Raw;
            set { if (value) MessageType = MessageType.Raw; }
        }

        public bool MessageTypeTapeti
        {
            get => MessageType == MessageType.Tapeti;
            set { if (value) MessageType = MessageType.Tapeti; }
        }


        public UserControl? MessageTypeControl
        {
            get => messageTypeControl;
            set => SetField(ref messageTypeControl, value);
        }


        public ICommand PublishCommand => publishCommand;


        public string Title => SendToQueue
                ? string.IsNullOrWhiteSpace(Queue) ? PublisherViewStrings.TabTitleEmpty : string.Format(PublisherViewStrings.TabTitle, Queue)
                : string.IsNullOrWhiteSpace(RoutingKey) ? PublisherViewStrings.TabTitleEmpty : string.Format(PublisherViewStrings.TabTitle, RoutingKey);


        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        string IPublishDestination.Exchange => SendToExchange ? Exchange : "";
        string IPublishDestination.RoutingKey => SendToExchange ? RoutingKey : Queue;


        public PublisherViewModel(ITabFactory tabFactory, IConnection connection, IExampleGenerator exampleGenerator, ReceivedMessageInfo? fromReceivedMessage = null)
        {
            this.connection = connection;
            this.exampleGenerator = exampleGenerator;
            this.tabFactory = tabFactory;

            publishCommand = new DelegateCommand(PublishExecute, PublishCanExecute);

            toolbarCommands = new[]
            {
                new TabToolbarCommand(PublishCommand, PublisherViewStrings.CommandPublish, SvgIconHelper.LoadFromResource("/Images/PublishSend.svg"))
            };

            if (fromReceivedMessage != null)
                SetMessageTypeControl(fromReceivedMessage);
            else
                SetMessageTypeControl(MessageType.Raw);
        }


        private void PublishExecute()
        {
            messageTypePublishCommand?.Execute(null);
        }


        private bool PublishCanExecute()
        {
            if (SendToExchange)
            {
                if (string.IsNullOrWhiteSpace(Exchange) || string.IsNullOrWhiteSpace(RoutingKey))
                    return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Queue))
                    return false;
            }

            return messageTypePublishCommand?.CanExecute(null) ?? false;
        }


        private void SetMessageTypeControl(MessageType value)
        {
            switch (value)
            {
                case MessageType.Raw:
                    RawPublisherViewModel rawPublisherViewModel;

                    if (rawPublisherView == null)
                    {
                        rawPublisherViewModel = new RawPublisherViewModel(connection, this);
                        rawPublisherViewModel.PublishCommand.CanExecuteChanged += (_, _) =>
                        {
                            publishCommand.RaiseCanExecuteChanged();
                        };

                        rawPublisherView ??= new RawPublisherView(rawPublisherViewModel);
                    }
                    else
                        rawPublisherViewModel = (RawPublisherViewModel)rawPublisherView.DataContext;

                    MessageTypeControl = rawPublisherView;

                    messageTypePublishCommand = rawPublisherViewModel.PublishCommand;
                    break;
                    
                case MessageType.Tapeti:
                    TapetiPublisherViewModel tapetiPublisherViewModel;

                    if (tapetiPublisherView == null)
                    {
                        tapetiPublisherViewModel = new TapetiPublisherViewModel(connection, this, exampleGenerator);
                        tapetiPublisherViewModel.PublishCommand.CanExecuteChanged += (_, _) =>
                        {
                            publishCommand.RaiseCanExecuteChanged();
                        };

                        tapetiPublisherView ??= new TapetiPublisherView(tapetiPublisherViewModel);

                        if (tabHostWindow != null)
                            tapetiPublisherViewModel.HostWindowChanged(tabHostWindow);
                    }
                    else
                        tapetiPublisherViewModel = (TapetiPublisherViewModel)tapetiPublisherView.DataContext;

                    MessageTypeControl = tapetiPublisherView;

                    messageTypePublishCommand = tapetiPublisherViewModel.PublishCommand;
                    break;
                
                default:
                    throw new ArgumentException($@"Unknown message type: {value}", nameof(value));
            }

            publishCommand.RaiseCanExecuteChanged();
        }


        private void SetMessageTypeControl(ReceivedMessageInfo fromReceivedMessage)
        {
            Exchange = fromReceivedMessage.Exchange;
            RoutingKey = fromReceivedMessage.RoutingKey;


            if (TapetiPublisherViewModel.IsTapetiMessage(fromReceivedMessage))
            {
                var tapetiPublisherViewModel = new TapetiPublisherViewModel(connection, this, exampleGenerator, fromReceivedMessage);
                tapetiPublisherView = new TapetiPublisherView(tapetiPublisherViewModel);

                MessageType = MessageType.Tapeti;
            }
            else
            {
                var rawPublisherViewModel = new RawPublisherViewModel(connection, this, fromReceivedMessage);
                rawPublisherView = new RawPublisherView(rawPublisherViewModel);

                MessageType = MessageType.Raw;
            }
        }


        public string? GetReplyTo(ref string? correlationId)
        {
            if (ReplyToSpecified)
                return string.IsNullOrEmpty(ReplyTo) ? null : ReplyTo;

            correlationId = PublisherViewStrings.ReplyToCorrelationIdPrefix + (SendToExchange ? RoutingKey : Queue);
            return tabFactory.CreateReplySubscriberTab(connection);
        }


        public void SetExchangeDestination(string newExchange, string newRoutingKey)
        {
            Exchange = newExchange;
            RoutingKey = newRoutingKey;
        }


        public void HostWindowChanged(Window? hostWindow)
        {
            tabHostWindow = hostWindow;

            (tapetiPublisherView?.DataContext as TapetiPublisherViewModel)?.HostWindowChanged(hostWindow);
        }
    }


    public class DesignTimePublisherViewModel : PublisherViewModel
    {
        public DesignTimePublisherViewModel() : base(null!, null!, null!)
        {
        }

        public override Visibility ExchangeVisibility => Visibility.Visible;
        public override Visibility QueueVisibility => Visibility.Visible;
    }
}
