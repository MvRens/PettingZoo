using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using IConnection = PettingZoo.Core.Connection.IConnection;

namespace PettingZoo.UI.Tab.Publisher
{
    public class TapetiPublisherViewModel : BaseViewModel
    {
        private readonly IConnection connection;
        private readonly IPublishDestination publishDestination;
        private readonly DelegateCommand publishCommand;

        private string correlationId = "";
        private string payload = "";
        private string className = "";
        private string assemblyName = "";


        public string CorrelationId
        {
            get => correlationId;
            set => SetField(ref correlationId, value);
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


        public TapetiPublisherViewModel(IConnection connection, IPublishDestination publishDestination, ReceivedMessageInfo? receivedMessage = null)
        {
            this.connection = connection;
            this.publishDestination = publishDestination;

            publishCommand = new DelegateCommand(PublishExecute, PublishCanExecute);


            if (receivedMessage == null || !IsTapetiMessage(receivedMessage, out var receivedAssemblyName, out var receivedClassName)) 
                return;

            AssemblyName = receivedAssemblyName;
            ClassName = receivedClassName;
            CorrelationId = receivedMessage.Properties.CorrelationId ?? "";
            Payload = Encoding.UTF8.GetString(receivedMessage.Body);
        }


        private void PublishExecute()
        {
            static string? NullIfEmpty(string? value)
            {
                return string.IsNullOrEmpty(value) ? null : value;
            }
            
            // TODO background worker / async

            connection.Publish(new PublishMessageInfo(
                publishDestination.Exchange,
                publishDestination.RoutingKey,
                Encoding.UTF8.GetBytes(Payload),
                new MessageProperties(new Dictionary<string, string>
                {
                    { @"classType", $"{ClassName}:{AssemblyName}" }
                })
                {
                    ContentType = @"application/json",
                    CorrelationId = NullIfEmpty(CorrelationId),
                    DeliveryMode = MessageDeliveryMode.Persistent,
                    ReplyTo = publishDestination.GetReplyTo()
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
        public DesignTimeTapetiPublisherViewModel() : base(null!, null!)
        {
            AssemblyName = "Messaging.Example";
            ClassName = "Messaging.Example.ExampleMessage";
            CorrelationId = "2c702859-bbbc-454e-87e2-4220c8c595d7";
            Payload = "{\r\n    \"Hello\": \"world!\"\r\n}";
        }
    }
}
