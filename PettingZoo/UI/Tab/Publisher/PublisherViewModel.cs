using System;
using System.Collections.Generic;
using System.Text;
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
    

    public class PublisherViewModel : BaseViewModel, ITabToolbarCommands
    {
        private readonly IConnection connection;

        private MessageType messageType;
        private UserControl? messageTypeControl;
        private ICommand? messageTypePublishCommand;

        private UserControl? rawPublisherView;
        private UserControl? tapetiPublisherView;

        private readonly DelegateCommand publishCommand;
        private readonly TabToolbarCommand[] toolbarCommands;


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
        public string Title => "Publish";
        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        public PublisherViewModel(IConnection connection, ReceivedMessageInfo? fromReceivedMessage = null)
        {
            this.connection = connection;

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
                    var rawPublisherViewModel = new RawPublisherViewModel(connection);
                    rawPublisherView ??= new RawPublisherView(rawPublisherViewModel);
                    MessageTypeControl = rawPublisherView;

                    messageTypePublishCommand = rawPublisherViewModel.PublishCommand;
                    break;
                    
                case MessageType.Tapeti:
                    var tapetiPublisherViewModel = new TapetiPublisherViewModel(connection);
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
            if (TapetiPublisherViewModel.IsTapetiMessage(fromReceivedMessage))
            {
                var tapetiPublisherViewModel = new TapetiPublisherViewModel(connection, fromReceivedMessage);
                tapetiPublisherView = new TapetiPublisherView(tapetiPublisherViewModel);

                SetMessageTypeControl(MessageType.Tapeti);
            }
            else
            {
                var rawPublisherViewModel = new RawPublisherViewModel(connection, fromReceivedMessage);
                rawPublisherView = new RawPublisherView(rawPublisherViewModel);

                SetMessageTypeControl(MessageType.Raw);
            }
        }
    }


    public class DesignTimePublisherViewModel : PublisherViewModel
    {
        public DesignTimePublisherViewModel() : base(null!)
        {
        }
    }
}
