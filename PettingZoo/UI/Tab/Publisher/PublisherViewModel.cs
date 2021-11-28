using System.Collections.Generic;
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

        private readonly DelegateCommand publishCommand;
        private readonly TabToolbarCommand[] toolbarCommands;


        public MessageType MessageType
        {
            get => messageType;
            set => SetField(ref messageType, value,
                otherPropertiesChanged: new[]
                {
                    nameof(MessageTypeRaw),
                    nameof(MessageTypeTapeti)
                });

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
        }


        private void PublishExecute()
        {
            // TODO
        }


        private bool PublishCanExecute()
        {
            // TODO validate input
            return true;
        }
    }


    public class DesignTimePublisherViewModel : PublisherViewModel
    {
        public DesignTimePublisherViewModel() : base(null!)
        {
        }
    }
}
