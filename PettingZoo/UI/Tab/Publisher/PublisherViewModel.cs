using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PettingZoo.Core.Connection;

namespace PettingZoo.UI.Tab.Publisher
{
    public enum MessageType
    {
        Raw,
        Tapeti
    }
    

    public class PublisherViewModel : BaseViewModel, ITabToolbarCommands, IPublishDestination
    {
        private readonly IConnection connection;
        private readonly ITabFactory tabFactory;
        private readonly ITabHost tabHost;

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


        public bool SendToExchange
        {
            get => sendToExchange;
            set => SetField(ref sendToExchange, value, otherPropertiesChanged: new[] { nameof(SendToQueue), nameof(ExchangeVisibility), nameof(QueueVisibility) });
        }


        public bool SendToQueue
        {
            get => !SendToExchange;
            set => SendToExchange = !value;
        }


        public string Exchange
        {
            get => exchange;
            set => SetField(ref exchange, value);
        }


        public string RoutingKey
        {
            get => routingKey;
            set => SetField(ref routingKey, value);
        }


        public string Queue
        {
            get => queue;
            set => SetField(ref queue, value);
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


        // TODO make more dynamic, include entered routing key for example
        #pragma warning disable CA1822 // Mark members as static - can't, it's part of the interface you silly, that would break the build
        public string Title => "Publish";
        #pragma warning restore CA1822
        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        string IPublishDestination.Exchange => SendToExchange ? Exchange : "";
        string IPublishDestination.RoutingKey => SendToExchange ? RoutingKey : Queue;


        public PublisherViewModel(ITabHost tabHost, ITabFactory tabFactory, IConnection connection, ReceivedMessageInfo? fromReceivedMessage = null)
        {
            this.connection = connection;
            this.tabFactory = tabFactory;
            this.tabHost = tabHost;

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
            return messageTypePublishCommand?.CanExecute(null) ?? false;
        }


        private void SetMessageTypeControl(MessageType value)
        {
            switch (value)
            {
                case MessageType.Raw:
                    var rawPublisherViewModel = new RawPublisherViewModel(connection, this);
                    rawPublisherView ??= new RawPublisherView(rawPublisherViewModel);
                    MessageTypeControl = rawPublisherView;

                    messageTypePublishCommand = rawPublisherViewModel.PublishCommand;
                    break;
                    
                case MessageType.Tapeti:
                    var tapetiPublisherViewModel = new TapetiPublisherViewModel(connection, this);
                    tapetiPublisherView ??= new TapetiPublisherView(tapetiPublisherViewModel);
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
                var tapetiPublisherViewModel = new TapetiPublisherViewModel(connection, this, fromReceivedMessage);
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


        public string? GetReplyTo()
        {
            if (ReplyToSpecified)
                return string.IsNullOrEmpty(ReplyTo) ? null : ReplyTo;

            var subscriber = connection.Subscribe();
            var tab = tabFactory.CreateSubscriberTab(connection, subscriber);
            tabHost.AddTab(tab);

            subscriber.Start();
            return subscriber.QueueName;
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
