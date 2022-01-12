using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Generator;
using PettingZoo.Core.Validation;
using PettingZoo.WPF.ViewModel;
using IConnection = PettingZoo.Core.Connection.IConnection;

namespace PettingZoo.UI.Tab.Publisher
{
    public class TapetiPublisherViewModel : BaseViewModel, ITabHostWindowNotify, IPayloadValidator
    {
        private readonly IConnection connection;
        private readonly IPublishDestination publishDestination;
        private readonly IExampleGenerator exampleGenerator;
        private readonly DelegateCommand publishCommand;
        private readonly DelegateCommand browseClassCommand;

        private string correlationId = "";
        private string payload = "";
        private string className = "";
        private string assemblyName = "";
        private Window? tabHostWindow;
        private IValidatingExample? validatingExample;


        public string CorrelationId
        {
            get => correlationId;
            set => SetField(ref correlationId, value);
        }


        public string ClassName
        {
            get => string.IsNullOrWhiteSpace(className) 
                ? string.IsNullOrWhiteSpace(AssemblyName) 
                    ? "" 
                    : AssemblyName + "." 
                : className;

            set
            {
                if (SetField(ref className, value, delegateCommandsChanged: new[] { publishCommand }))
                    validatingExample = null;
            }
        }


        public string AssemblyName
        {
            get => assemblyName;
            set => SetField(ref assemblyName, value, delegateCommandsChanged: new[] { publishCommand }, otherPropertiesChanged:
                string.IsNullOrEmpty(value) || string.IsNullOrEmpty(className)
                    ? new [] { nameof(ClassName) } 
                    : null
                );
        }


        public string Payload
        {
            get => payload;
            set => SetField(ref payload, value, delegateCommandsChanged: new[] { publishCommand });
        }


        public ICommand PublishCommand => publishCommand;
        public ICommand BrowseClassCommand => browseClassCommand;



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


        public TapetiPublisherViewModel(IConnection connection, IPublishDestination publishDestination, IExampleGenerator exampleGenerator, ReceivedMessageInfo? receivedMessage = null)
        {
            this.connection = connection;
            this.publishDestination = publishDestination;
            this.exampleGenerator = exampleGenerator;

            publishCommand = new DelegateCommand(PublishExecute, PublishCanExecute);
            browseClassCommand = new DelegateCommand(BrowseClassExecute);


            if (receivedMessage == null || !IsTapetiMessage(receivedMessage, out var receivedAssemblyName, out var receivedClassName)) 
                return;

            AssemblyName = receivedAssemblyName;
            ClassName = receivedClassName;
            CorrelationId = receivedMessage.Properties.CorrelationId ?? "";
            Payload = Encoding.UTF8.GetString(receivedMessage.Body);
        }


        private void BrowseClassExecute()
        {
            exampleGenerator.Select(tabHostWindow, example =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    switch (example)
                    {
                        case null:
                            return;

                        case IClassTypeExample classTypeExample:
                            AssemblyName = classTypeExample.AssemblyName;
                            ClassName = classTypeExample.FullClassName;

                            validatingExample = classTypeExample as IValidatingExample;
                            break;
                    }

                    Payload = example.Generate();
                });
            });
        }


        private void PublishExecute()
        {
            static string? NullIfEmpty(string? value)
            {
                return string.IsNullOrEmpty(value) ? null : value;
            }

            var publishCorrelationId = NullIfEmpty(CorrelationId);
            var replyTo = publishDestination.GetReplyTo(ref publishCorrelationId);

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
                    CorrelationId = publishCorrelationId,
                    DeliveryMode = MessageDeliveryMode.Persistent,
                    ReplyTo = replyTo
                }));
        }


        private bool PublishCanExecute()
        {
            return
                !string.IsNullOrWhiteSpace(assemblyName) &&
                !string.IsNullOrWhiteSpace(ClassName) && 
                !string.IsNullOrWhiteSpace(Payload);
        }


        public void HostWindowChanged(Window? hostWindow)
        {
            tabHostWindow = hostWindow;
        }


        public bool CanValidate()
        {
            return validatingExample != null && validatingExample.CanValidate();
        }


        public void Validate(string validatePayload)
        {
            validatingExample?.Validate(validatePayload);
        }
    }


    public class DesignTimeTapetiPublisherViewModel : TapetiPublisherViewModel
    {
        public DesignTimeTapetiPublisherViewModel() : base(null!, null!, null!)
        {
            AssemblyName = "Messaging.Example";
            ClassName = "Messaging.Example.ExampleMessage";
            CorrelationId = "2c702859-bbbc-454e-87e2-4220c8c595d7";
            Payload = "{\r\n    \"Hello\": \"world!\"\r\n}";
        }
    }
}
