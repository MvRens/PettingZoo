using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using IConnection = PettingZoo.Core.Connection.IConnection;

namespace PettingZoo.UI.Tab.Publisher
{
    public class TapetiPublisherViewModel : BaseViewModel
    {
        private readonly IConnection connection;
        private readonly DelegateCommand publishCommand;

        private bool sendToExchange = true;
        private string exchange = "";
        private string routingKey = "";
        private string queue = "";

        private MessageDeliveryMode deliveryMode;

        private string correlationId = "";
        private string replyTo = "";
        private string payload = "";
        private string className = "";
        private string assemblyName = "";


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


        public virtual Visibility ExchangeVisibility => SendToExchange ? Visibility.Visible : Visibility.Collapsed;
        public virtual Visibility QueueVisibility => SendToQueue ? Visibility.Visible : Visibility.Collapsed;


        public int DeliveryModeIndex
        {
            get => deliveryMode == MessageDeliveryMode.Persistent ? 1 : 0;
            set => SetField(ref deliveryMode, value == 1 ? MessageDeliveryMode.Persistent : MessageDeliveryMode.NonPersistent);
        }


        public string CorrelationId
        {
            get => correlationId;
            set => SetField(ref correlationId, value);
        }


        public string ReplyTo
        {
            get => replyTo;
            set => SetField(ref replyTo, value);
        }


        public string ClassName
        {
            get => string.IsNullOrEmpty(className) ? AssemblyName + "." : className;
            set => SetField(ref className, value);
        }


        public string AssemblyName
        {
            get => assemblyName;
            set => SetField(ref assemblyName, value, otherPropertiesChanged:
                string.IsNullOrEmpty(value) || string.IsNullOrEmpty(className)
                    ? new [] { nameof(ClassName) } 
                    : null
                );
        }


        public string Payload
        {
            get => payload;
            set => SetField(ref payload, value);
        }


        public ICommand PublishCommand => publishCommand;



        public static bool IsTapetiMessage(ReceivedMessageInfo receivedMessage)
        {
            return IsTapetiMessage(receivedMessage, out _, out _);
        }


        public static bool IsTapetiMessage(ReceivedMessageInfo receivedMessage, out string assemblyName, out string className)
        {
            assemblyName = "";
            className = "";

            if (receivedMessage.Properties.ContentType != @"application/json")
                return false;

            if (!receivedMessage.Properties.Headers.TryGetValue(@"classType", out var classType))
                return false;

            var parts = classType.Split(':');
            if (parts.Length != 2)
                return false;

            className = parts[0];
            assemblyName = parts[1];
            return true;
        }


        public TapetiPublisherViewModel(IConnection connection, ReceivedMessageInfo? receivedMessage = null)
        {
            this.connection = connection;

            publishCommand = new DelegateCommand(PublishExecute, PublishCanExecute);


            if (receivedMessage == null) 
                return;

            Exchange = receivedMessage.Exchange;
            RoutingKey = receivedMessage.RoutingKey;

            AssemblyName = assemblyName;
            ClassName = className;
            CorrelationId = receivedMessage.Properties.CorrelationId ?? "";
            ReplyTo = receivedMessage.Properties.ReplyTo ?? "";
            Payload = Encoding.UTF8.GetString(receivedMessage.Body);
        }


        private void PublishExecute()
        {
            static string? NullIfEmpty(string? value)
            {
                return string.IsNullOrEmpty(value) ? null : value;
            }
            
            // TODO support for Reply To to dynamic queue which waits for a message (or opens a new subscriber tab?)
            // TODO background worker / async

            connection.Publish(new PublishMessageInfo(
                SendToExchange ? Exchange : "", 
                SendToExchange ? RoutingKey : Queue,
                Encoding.UTF8.GetBytes(Payload),
                new MessageProperties(new Dictionary<string, string>
                {
                    { @"classType", $"{ClassName}:{AssemblyName}" }
                })
                {
                    ContentType = @"application/json",
                    CorrelationId = NullIfEmpty(CorrelationId),
                    DeliveryMode = deliveryMode,
                    ReplyTo = NullIfEmpty(ReplyTo)
                }));
        }


        private bool PublishCanExecute()
        {
            // TODO validate input
            return true;
        }
    }


    public class DesignTimeTapetiPublisherViewModel : TapetiPublisherViewModel
    {
        public DesignTimeTapetiPublisherViewModel() : base(null!)
        {
        }


        public override Visibility ExchangeVisibility => Visibility.Visible;
        public override Visibility QueueVisibility => Visibility.Visible;
    }
}
