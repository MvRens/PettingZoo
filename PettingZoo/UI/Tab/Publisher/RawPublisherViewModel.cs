using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PettingZoo.Core.Connection;
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



        public int DeliveryModeIndex
        {
            get => deliveryMode == MessageDeliveryMode.Persistent ? 1 : 0;
            set => SetField(ref deliveryMode, value == 1 ? MessageDeliveryMode.Persistent : MessageDeliveryMode.NonPersistent);
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
            set => SetField(ref payload, value);
        }


        public ObservableCollection<Header> Headers { get; } = new();


        public ICommand PublishCommand => publishCommand;
        public ICommand PropertiesExpandCollapseCommand => propertiesExpandCollapseCommand;


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


        public RawPublisherViewModel(IConnection connection, IPublishDestination publishDestination, BaseMessageInfo? receivedMessage = null)
        {
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
            
            // TODO check parsing of priority and timestamp
            
            var headers = Headers.Where(h => h.IsValid()).ToDictionary(h => h.Key, h => h.Value);

            // TODO background worker / async

            connection.Publish(new PublishMessageInfo(
                publishDestination.Exchange, 
                publishDestination.RoutingKey,
                Encoding.UTF8.GetBytes(Payload),
                new MessageProperties(headers)
                {
                    AppId = NullIfEmpty(AppId),
                    ContentEncoding = NullIfEmpty(ContentEncoding),
                    ContentType = NullIfEmpty(ContentType),
                    CorrelationId = NullIfEmpty(CorrelationId),
                    DeliveryMode = deliveryMode,
                    Expiration = NullIfEmpty(Expiration),
                    MessageId = NullIfEmpty(MessageId),
                    Priority = !string.IsNullOrEmpty(Priority) && byte.TryParse(Priority, out var priorityValue) ? priorityValue : null,
                    ReplyTo = publishDestination.GetReplyTo(),
                    Timestamp = !string.IsNullOrEmpty(Timestamp) && DateTime.TryParse(Timestamp, out var timestampValue) ? timestampValue : null,
                    Type = NullIfEmpty(TypeProperty),
                    UserId = NullIfEmpty(UserId)
                }));
        }


        private static bool PublishCanExecute()
        {
            // TODO validate input
            return true;
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
        public DesignTimeRawPublisherViewModel() : base(null!, null!)
        {
            PropertiesExpanded = true;

            var capturedLastHeader = LastHeader;
            capturedLastHeader.Key = "Example";
            capturedLastHeader.Value = "header";
        }
    }
}
