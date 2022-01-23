using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.ExportImport.Publisher;
using PettingZoo.Core.Generator;
using PettingZoo.Core.Macros;
using PettingZoo.Core.Settings;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Tab.Publisher
{
    public class PublisherViewModel : BaseViewModel, ITabToolbarCommands, ITabHostWindowNotify, IPublishDestination
    {
        private readonly IConnection connection;
        private readonly IExampleGenerator exampleGenerator;
        private readonly IPayloadMacroProcessor payloadMacroProcessor;
        private readonly StoredPublisherMessagesViewModel storedPublisherMessagesViewModel;
        private readonly ITabFactory tabFactory;

        private bool sendToExchange = true;
        private string exchange = "";
        private string routingKey = "";
        private string queue = "";
        private string replyTo = "";
        private bool replyToNewSubscriber;

        private StoredPublisherMessage? selectedStoredMessage;
        private StoredPublisherMessage? activeStoredMessage;

        private PublisherMessageType messageType;
        private UserControl? messageTypeControl;
        private ICommand? messageTypePublishCommand;

        private RawPublisherViewModel? rawPublisherViewModel;
        private UserControl? rawPublisherView;

        private TapetiPublisherViewModel? tapetiPublisherViewModel;
        private UserControl? tapetiPublisherView;

        private readonly DelegateCommand publishCommand;
        private readonly DelegateCommand saveCommand;
        private readonly DelegateCommand saveAsCommand;
        private readonly DelegateCommand deleteCommand;
        private readonly DelegateCommand loadStoredMessage;

        private readonly TabToolbarCommand[] toolbarCommands;
        private Window? tabHostWindow;


        public bool SendToExchange
        {
            get => sendToExchange;
            set => SetField(ref sendToExchange, value,
                delegateCommandsChanged: new[] { publishCommand },
                otherPropertiesChanged: new[]
                    { nameof(SendToQueue), nameof(ExchangeVisibility), nameof(QueueVisibility), nameof(Title) });
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
            set => SetField(ref routingKey, value, delegateCommandsChanged: new[] { publishCommand },
                otherPropertiesChanged: new[] { nameof(Title) });
        }


        public string Queue
        {
            get => queue;
            set => SetField(ref queue, value, delegateCommandsChanged: new[] { publishCommand },
                otherPropertiesChanged: new[] { nameof(Title) });
        }


        public string ReplyTo
        {
            get => replyTo;
            set => SetField(ref replyTo, value);
        }


        public bool ReplyToSpecified
        {
            get => !ReplyToNewSubscriber;
            set => ReplyToNewSubscriber = !value;
        }


        public bool ReplyToNewSubscriber
        {
            get => replyToNewSubscriber;
            set => SetField(ref replyToNewSubscriber, value,
                otherPropertiesChanged: new[] { nameof(ReplyToSpecified) });
        }


        public virtual Visibility ExchangeVisibility => SendToExchange ? Visibility.Visible : Visibility.Collapsed;
        public virtual Visibility QueueVisibility => SendToQueue ? Visibility.Visible : Visibility.Collapsed;


        public PublisherMessageType MessageType
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
            get => MessageType == PublisherMessageType.Raw;
            set
            {
                if (value) MessageType = PublisherMessageType.Raw;
            }
        }

        public bool MessageTypeTapeti
        {
            get => MessageType == PublisherMessageType.Tapeti;
            set
            {
                if (value) MessageType = PublisherMessageType.Tapeti;
            }
        }


        public UserControl? MessageTypeControl
        {
            get => messageTypeControl;
            set => SetField(ref messageTypeControl, value);
        }


        public ObservableCollectionEx<StoredPublisherMessage> StoredMessages =>
            storedPublisherMessagesViewModel.StoredMessages;

        public StoredPublisherMessage? SelectedStoredMessage
        {
            get => selectedStoredMessage;
            set => SetField(ref selectedStoredMessage, value, delegateCommandsChanged: new[] { deleteCommand });
        }


        // TODO detect changes from ActiveStoredMessage and show indication in the UI

        public StoredPublisherMessage? ActiveStoredMessage
        {
            get => activeStoredMessage;
            set => SetField(ref activeStoredMessage, value);
        }


        public ICommand PublishCommand => publishCommand;
        public ICommand SaveCommand => saveCommand;
        public ICommand SaveAsCommand => saveAsCommand;
        public ICommand DeleteCommand => deleteCommand;
        public ICommand LoadStoredMessage => loadStoredMessage;


        public string Title => SendToQueue
            ? string.IsNullOrWhiteSpace(Queue) ? PublisherViewStrings.TabTitleEmpty :
            string.Format(PublisherViewStrings.TabTitle, Queue)
            : string.IsNullOrWhiteSpace(RoutingKey)
                ? PublisherViewStrings.TabTitleEmpty
                : string.Format(PublisherViewStrings.TabTitle, RoutingKey);


        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        string IPublishDestination.Exchange => SendToExchange ? Exchange : "";
        string IPublishDestination.RoutingKey => SendToExchange ? RoutingKey : Queue;


        public PublisherViewModel(ITabFactory tabFactory, IConnection connection, IExampleGenerator exampleGenerator,
            IPayloadMacroProcessor payloadMacroProcessor,
            StoredPublisherMessagesViewModel storedPublisherMessagesViewModel,
            ReceivedMessageInfo? fromReceivedMessage = null)
        {
            this.connection = connection;
            this.exampleGenerator = exampleGenerator;
            this.payloadMacroProcessor = payloadMacroProcessor;
            this.storedPublisherMessagesViewModel = storedPublisherMessagesViewModel;
            this.tabFactory = tabFactory;

            publishCommand = new DelegateCommand(PublishExecute, PublishCanExecute);
            saveCommand = new DelegateCommand(SaveExecute);
            saveAsCommand = new DelegateCommand(SaveAsExecute);
            deleteCommand = new DelegateCommand(DeleteExecute, DeleteCanExecute);
            loadStoredMessage = new DelegateCommand(LoadStoredMessageExecute, LoadStoredMessageCanExecute);

            toolbarCommands = new[]
            {
                new TabToolbarCommand(PublishCommand, PublisherViewStrings.CommandPublish,
                    SvgIconHelper.LoadFromResource("/Images/PublishSend.svg"))
            };

            if (fromReceivedMessage != null)
                SetMessageTypeControl(fromReceivedMessage);
            else
                SetMessageTypeControl(PublisherMessageType.Raw);
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


        private void SetMessageTypeControl(PublisherMessageType value)
        {
            switch (value)
            {
                case PublisherMessageType.Raw:
                    if (rawPublisherView == null)
                    {
                        rawPublisherViewModel = new RawPublisherViewModel(connection, this, payloadMacroProcessor);
                        rawPublisherViewModel.PublishCommand.CanExecuteChanged += (_, _) =>
                        {
                            publishCommand.RaiseCanExecuteChanged();
                        };

                        rawPublisherView = new RawPublisherView(rawPublisherViewModel);
                    }
                    else
                        Debug.Assert(rawPublisherViewModel != null);

                    MessageTypeControl = rawPublisherView;

                    messageTypePublishCommand = rawPublisherViewModel.PublishCommand;
                    break;

                case PublisherMessageType.Tapeti:
                    if (tapetiPublisherView == null)
                    {
                        tapetiPublisherViewModel = new TapetiPublisherViewModel(connection, this, exampleGenerator, payloadMacroProcessor);
                        tapetiPublisherViewModel.PublishCommand.CanExecuteChanged += (_, _) =>
                        {
                            publishCommand.RaiseCanExecuteChanged();
                        };

                        tapetiPublisherView = new TapetiPublisherView(tapetiPublisherViewModel);

                        if (tabHostWindow != null)
                            tapetiPublisherViewModel.HostWindowChanged(tabHostWindow);
                    }
                    else
                        Debug.Assert(tapetiPublisherViewModel != null);

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
                tapetiPublisherViewModel = new TapetiPublisherViewModel(connection, this, exampleGenerator, payloadMacroProcessor, fromReceivedMessage);
                tapetiPublisherView = new TapetiPublisherView(tapetiPublisherViewModel);

                MessageType = PublisherMessageType.Tapeti;
            }
            else
            {
                rawPublisherViewModel = new RawPublisherViewModel(connection, this, payloadMacroProcessor, fromReceivedMessage);
                rawPublisherView = new RawPublisherView(rawPublisherViewModel);

                MessageType = PublisherMessageType.Raw;
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


        private void SaveExecute()
        {
            storedPublisherMessagesViewModel.Save(SelectedStoredMessage, GetPublisherMessage(), message =>
            {
                ActiveStoredMessage = message;
                SelectedStoredMessage = message;
            });
        }


        private void SaveAsExecute()
        {
            storedPublisherMessagesViewModel.SaveAs(GetPublisherMessage(), SelectedStoredMessage?.DisplayName, message =>
            {
                ActiveStoredMessage = message;
                SelectedStoredMessage = message;
            });
        }


        private void DeleteExecute()
        {
            if (SelectedStoredMessage == null)
                return;

            var message = SelectedStoredMessage;

            storedPublisherMessagesViewModel.Delete(message, () =>
            {
                if (SelectedStoredMessage == message)
                    SelectedStoredMessage = null;

                if (ActiveStoredMessage == message)
                    ActiveStoredMessage = null;
            });
        }


        private bool DeleteCanExecute()
        {
            return SelectedStoredMessage != null;
        }


        private void LoadStoredMessageExecute()
        {
            if (SelectedStoredMessage == null)
                return;

            var message = SelectedStoredMessage.Message;

            MessageType = message.MessageType;
            SendToExchange = message.SendToExchange;
            Exchange = message.Exchange ?? "";
            RoutingKey = message.RoutingKey ?? "";
            Queue = message.Queue ?? "";
            ReplyToNewSubscriber = message.ReplyToNewSubscriber;
            ReplyTo = message.ReplyTo ?? "";

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (message.MessageType)
            {
                case PublisherMessageType.Raw:
                    if (message.RawPublisherMessage != null)
                        rawPublisherViewModel?.LoadPublisherMessage(message.RawPublisherMessage);

                    break;

                case PublisherMessageType.Tapeti:
                    if (message.TapetiPublisherMessage != null)
                        tapetiPublisherViewModel?.LoadPublisherMessage(message.TapetiPublisherMessage);

                    break;
            }

            ActiveStoredMessage = SelectedStoredMessage;
        }


        private bool LoadStoredMessageCanExecute()
        {
            return SelectedStoredMessage != null;
        }


        private PublisherMessage GetPublisherMessage()
        {
            return new PublisherMessage
            {
                MessageType = MessageType,
                SendToExchange = SendToExchange,
                Exchange = Exchange,
                RoutingKey = RoutingKey,
                Queue = Queue,
                ReplyToNewSubscriber = ReplyToNewSubscriber,
                ReplyTo = ReplyTo,

                RawPublisherMessage = MessageType == PublisherMessageType.Raw
                    ? rawPublisherViewModel?.GetPublisherMessage()
                    : null,

                TapetiPublisherMessage = MessageType == PublisherMessageType.Tapeti
                    ? tapetiPublisherViewModel?.GetPublisherMessage()
                    : null
            };
        }
    }


    public class DesignTimePublisherViewModel : PublisherViewModel
    {
        public DesignTimePublisherViewModel() : base(null!, null!, null!, null!, new StoredPublisherMessagesViewModel(new DesignTimePublisherMessagesRepository()))
        {
            StoredMessages.CollectionChanged += (_, _) =>
            {
                if (StoredMessages.Count < 2)
                    return;

                SelectedStoredMessage = StoredMessages[0];
                ActiveStoredMessage = StoredMessages[1];
            };
        }

        public override Visibility ExchangeVisibility => Visibility.Visible;
        public override Visibility QueueVisibility => Visibility.Visible;


        private class DesignTimePublisherMessagesRepository : IPublisherMessagesRepository
        {
            public Task<IEnumerable<StoredPublisherMessage>> GetStored()
            {
                return Task.FromResult(new StoredPublisherMessage[]
                {
                    new(new Guid("16fdf930-2e4c-48f4-ae21-68dac9ca62e6"), "Design-time message 1", new PublisherMessage()),
                    new(new Guid("01d2671b-4426-4c1c-bcbc-61689d14796e"), "Design-time message 2", new PublisherMessage())
                } as IEnumerable<StoredPublisherMessage>);
            }

            public Task<StoredPublisherMessage> Add(string displayName, PublisherMessage message)
            {
                throw new NotSupportedException();
            }

            public Task<StoredPublisherMessage> Update(Guid id, string displayName, PublisherMessage message)
            {
                throw new NotSupportedException();
            }

            public Task Delete(Guid id)
            {
                throw new NotSupportedException();
            }
        }
    }
}
