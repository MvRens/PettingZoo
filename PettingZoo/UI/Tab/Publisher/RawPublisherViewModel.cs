using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.ExportImport.Publisher;
using PettingZoo.Core.Macros;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Tab.Publisher
{
    public class RawPublisherViewModel : BaseViewModel
    {
        private readonly IConnection connection;
        private readonly IPublishDestination publishDestination;
        private readonly DelegateCommand publishCommand;
        private readonly DelegateCommand propertiesExpandCollapseCommand;
        private bool propertiesExpanded;

        private MessageDeliveryMode deliveryMode;

        private string contentType = "application/json";
        private string correlationId = "";
        private string appId = "";
        private string contentEncoding = "";
        private string expiration = "";
        private string messageId = "";
        private string priority = "";
        private string timestamp = "";
        private string typeProperty = "";
        private string userId = "";
        private string payload = "";
        private bool enableMacros;



        public MessageDeliveryMode DeliveryMode
        {
            get => deliveryMode;
            set => SetField(ref deliveryMode, value, otherPropertiesChanged: new[] { nameof(DeliveryModeIndex) });
        }


        public int DeliveryModeIndex
        {
            get => DeliveryMode == MessageDeliveryMode.Persistent ? 1 : 0;
            set => DeliveryMode = value == 1 ? MessageDeliveryMode.Persistent : MessageDeliveryMode.NonPersistent;
        }


        public string ContentType
        {
            get => contentType;
            set => SetField(ref contentType, value);
        }


        public string CorrelationId
        {
            get => correlationId;
            set => SetField(ref correlationId, value);
        }


        public string AppId
        {
            get => appId;
            set => SetField(ref appId, value);
        }


        public string ContentEncoding
        {
            get => contentEncoding;
            set => SetField(ref contentEncoding, value);
        }


        public string Expiration
        {
            get => expiration;
            set => SetField(ref expiration, value);
        }


        public string MessageId
        {
            get => messageId;
            set => SetField(ref messageId, value);
        }


        public string Priority
        {
            get => priority;
            set => SetField(ref priority, value);
        }


        public string Timestamp
        {
            get => timestamp;
            set => SetField(ref timestamp, value);
        }


        public string TypeProperty
        {
            get => typeProperty;
            set => SetField(ref typeProperty, value);
        }


        public string UserId
        {
            get => userId;
            set => SetField(ref userId, value);
        }


        public string Payload
        {
            get => payload;
            set => SetField(ref payload, value, delegateCommandsChanged: new [] { publishCommand });
        }

        public bool EnableMacros
        {
            get => enableMacros;
            set => SetField(ref enableMacros, value);
        }


        public ObservableCollectionEx<Header> Headers { get; } = new();


        public ICommand PublishCommand => publishCommand;
        public ICommand PropertiesExpandCollapseCommand => propertiesExpandCollapseCommand;

        public IPayloadMacroProcessor PayloadMacroProcessor { get; }


        public bool PropertiesExpanded
        {
            get => propertiesExpanded;
            set => SetField(ref propertiesExpanded, value, otherPropertiesChanged: new[]
            {
                nameof(PropertiesExpandedVisibility),
                nameof(PropertiesExpandedCollapsedText)
            });
        }

        public Visibility PropertiesExpandedVisibility => propertiesExpanded ? Visibility.Visible : Visibility.Collapsed;
        public string PropertiesExpandedCollapsedText => propertiesExpanded
            ? RawPublisherViewStrings.PropertiesCollapse
            : RawPublisherViewStrings.PropertiesExpand;


        protected Header LastHeader;


        public RawPublisherViewModel(IConnection connection, IPublishDestination publishDestination, IPayloadMacroProcessor payloadMacroProcessor, BaseMessageInfo? receivedMessage = null)
        {
            PayloadMacroProcessor = payloadMacroProcessor;

            this.connection = connection;
            this.publishDestination = publishDestination;

            publishCommand = new DelegateCommand(PublishExecute, PublishCanExecute);
            propertiesExpandCollapseCommand = new DelegateCommand(PropertiesExpandCollapseExecute);

            if (receivedMessage != null)
            {
                CorrelationId = receivedMessage.Properties.CorrelationId ?? "";
                Priority = receivedMessage.Properties.Priority?.ToString() ?? "";
                AppId = receivedMessage.Properties.AppId ?? "";
                ContentEncoding = receivedMessage.Properties.ContentEncoding ?? "";
                ContentType = receivedMessage.Properties.ContentType ?? "";
                Expiration = receivedMessage.Properties.Expiration ?? "";
                MessageId = receivedMessage.Properties.MessageId ?? "";
                Timestamp = receivedMessage.Properties.Timestamp?.ToString() ?? "";
                TypeProperty = receivedMessage.Properties.Type ?? "";
                UserId = receivedMessage.Properties.UserId ?? "";

                Payload = Encoding.UTF8.GetString(receivedMessage.Body);

                foreach (var (key, value) in receivedMessage.Properties.Headers)
                    Headers.Add(new Header
                    {
                        Key = key,
                        Value = value
                    });

                PropertiesExpanded = AnyNotEmpty(AppId, ContentEncoding, Expiration, MessageId, Priority, Timestamp, TypeProperty, UserId);
            }

            AddHeader();
        }


        public RawPublisherMessage GetPublisherMessage()
        {
            return new RawPublisherMessage
            {
                DeliveryMode = DeliveryMode,
                ContentType = ContentType,
                CorrelationId = CorrelationId,
                AppId = AppId,
                ContentEncoding = ContentEncoding,
                Expiration = Expiration,
                MessageId = MessageId,
                Priority = Priority,
                Timestamp = Timestamp,
                TypeProperty = TypeProperty,
                UserId = UserId,
                Payload = Payload,
                EnableMacros = EnableMacros,

                Headers = Headers.Where(h => !h.IsEmpty()).ToDictionary(h => h.Key, h => h.Value)
            };
        }


        public void LoadPublisherMessage(RawPublisherMessage message)
        {
            DeliveryMode = message.DeliveryMode;
            ContentType = message.ContentType ?? "";
            CorrelationId = message.CorrelationId ?? "";
            AppId = message.AppId ?? "";
            ContentEncoding = message.ContentEncoding ?? "";
            Expiration = message.Expiration ?? "";
            MessageId = message.MessageId ?? "";
            Priority = message.Priority ?? "";
            Timestamp = message.Timestamp ?? "";
            TypeProperty = message.TypeProperty ?? "";
            UserId = message.UserId ?? "";
            Payload = message.Payload ?? "";
            EnableMacros = message.EnableMacros;

            if (message.Headers != null)
            {
                Headers.ReplaceAll(message.Headers.Select(p => new Header
                {
                    Key = p.Key,
                    Value = p.Value
                }));
            }
            else
                Headers.Clear();

            AddHeader();
        }


        private static bool AnyNotEmpty(params string?[] values)
        {
            return values.Any(s => !string.IsNullOrEmpty(s));
        }


        private void LastHeaderChanged(object? sender, PropertyChangedEventArgs e)
        {
            LastHeader.PropertyChanged -= LastHeaderChanged;
            AddHeader();
        }


        [MemberNotNull(nameof(LastHeader))]
        private void AddHeader()
        {
            LastHeader = new Header();
            LastHeader.PropertyChanged += LastHeaderChanged;
            Headers.Add(LastHeader);
        }


        private void PropertiesExpandCollapseExecute()
        {
            PropertiesExpanded = !PropertiesExpanded;
        }


        private void PublishExecute()
        {
            static string? NullIfEmpty(string? value)
            {
                return string.IsNullOrEmpty(value) ? null : value;
            }

            byte? priorityValue = null;
            DateTime? timestampValue = null;

            if (!string.IsNullOrWhiteSpace(Priority))
            {
                if (byte.TryParse(Priority, out var priorityParsedValue))
                    priorityValue = priorityParsedValue;
                else
                {
                    MessageBox.Show(RawPublisherViewStrings.PriorityParseFailed, RawPublisherViewStrings.PublishValidationErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(Timestamp))
            {
                if (DateTime.TryParse(Timestamp, out var timestampParsedValue))
                    timestampValue = timestampParsedValue;
                else
                {
                    MessageBox.Show(RawPublisherViewStrings.TimestampParseFailed, RawPublisherViewStrings.PublishValidationErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var encodedPayload = Encoding.UTF8.GetBytes(
                EnableMacros
                    ? PayloadMacroProcessor.Apply(Payload)
                    : Payload
            );

            var headers = Headers.Where(h => h.IsValid()).ToDictionary(h => h.Key, h => h.Value);
            var publishCorrelationId = NullIfEmpty(CorrelationId);
            var replyTo = publishDestination.GetReplyTo(ref publishCorrelationId);

            connection.Publish(new PublishMessageInfo(
                publishDestination.Exchange, 
                publishDestination.RoutingKey,
                encodedPayload,
                new MessageProperties(headers)
                {
                    AppId = NullIfEmpty(AppId),
                    ContentEncoding = NullIfEmpty(ContentEncoding),
                    ContentType = NullIfEmpty(ContentType),
                    CorrelationId = publishCorrelationId,
                    DeliveryMode = deliveryMode,
                    Expiration = NullIfEmpty(Expiration),
                    MessageId = NullIfEmpty(MessageId),
                    Priority = priorityValue,
                    ReplyTo = replyTo,
                    Timestamp = timestampValue,
                    Type = NullIfEmpty(TypeProperty),
                    UserId = NullIfEmpty(UserId)
                }));
        }


        private bool PublishCanExecute()
        {
            return !string.IsNullOrWhiteSpace(Payload);
        }


        public class Header : BaseViewModel
        {
            private string key = "";
            private string value = "";


            public string Key
            {
                get => key;
                set => SetField(ref key, value);
            }


            public string Value
            {
                get => value;
                set => SetField(ref this.value, value);
            }


            public bool IsEmpty()
            {
                return string.IsNullOrEmpty(Key) && string.IsNullOrEmpty(Value);
            }


            public bool IsValid()
            {
                return !string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(Value);
            }
        }
    }


    public class DesignTimeRawPublisherViewModel : RawPublisherViewModel
    {
        public DesignTimeRawPublisherViewModel() : base(null!, null!, null!)
        {
            PropertiesExpanded = true;

            var capturedLastHeader = LastHeader;
            capturedLastHeader.Key = "Example";
            capturedLastHeader.Value = "header";
        }
    }
}
