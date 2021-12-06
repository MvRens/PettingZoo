using System;
using System.Collections.Generic;
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


        public PublisherViewModel(IConnection connection)
        {
            this.connection = connection;

            publishCommand = new DelegateCommand(PublishExecute, PublishCanExecute);

            toolbarCommands = new[]
            {
                new TabToolbarCommand(PublishCommand, PublisherViewStrings.CommandPublish, SvgIconHelper.LoadFromResource("/Images/PublishSend.svg"))
            };

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
                    publishCommand.RaiseCanExecuteChanged();
                    break;
                    
                case MessageType.Tapeti:
                    // TODO
                    var tapetiPublisherViewModel = new RawPublisherViewModel(connection);
                    tapetiPublisherView ??= new RawPublisherView(tapetiPublisherViewModel);
                    MessageTypeControl = tapetiPublisherView;

                    messageTypePublishCommand = tapetiPublisherViewModel.PublishCommand;
                    publishCommand.RaiseCanExecuteChanged();
                    break;
                
                default:
                    throw new ArgumentException();
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
