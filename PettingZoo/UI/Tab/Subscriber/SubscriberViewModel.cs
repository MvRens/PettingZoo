using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Rendering;

// TODO update title with unread message count if tab is not active
// TODO export option (to Tapeti.Cmd compatible format / command-line of course)
// TODO send message to (new) publisher tab

namespace PettingZoo.UI.Tab.Subscriber
{
    public class SubscriberViewModel : BaseViewModel, ITabToolbarCommands
    {
        private readonly ISubscriber subscriber;
        private readonly TaskScheduler uiScheduler;
        private ReceivedMessageInfo? selectedMessage;
        private readonly DelegateCommand clearCommand;
        private readonly TabToolbarCommand[] toolbarCommands;
        private IDictionary<string, string>? selectedMessageProperties;


        public ICommand ClearCommand => clearCommand;

        public ObservableCollection<ReceivedMessageInfo> Messages { get; }

        public ReceivedMessageInfo? SelectedMessage
        {
            get => selectedMessage;
            set
            {
                if (SetField(ref selectedMessage, value, otherPropertiesChanged: new[] { nameof(SelectedMessageBody) }))
                    UpdateSelectedMessageProperties();
            }
        }

        public string SelectedMessageBody =>
            SelectedMessage != null
                ? MessageBodyRenderer.Render(SelectedMessage.Body, SelectedMessage.Properties.ContentType)
                : "";

        public IDictionary<string, string>? SelectedMessageProperties
        {
            get => selectedMessageProperties;
            set => SetField(ref selectedMessageProperties, value);
        }

        public string Title => $"{subscriber.Exchange} - {subscriber.RoutingKey}";
        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        public SubscriberViewModel(ISubscriber subscriber)
        {
            this.subscriber = subscriber;
            
            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Messages = new ObservableCollection<ReceivedMessageInfo>();
            clearCommand = new DelegateCommand(ClearExecute, ClearCanExecute);

            toolbarCommands = new[]
            {
                new TabToolbarCommand(ClearCommand, SubscriberViewStrings.CommandClear, SvgIconHelper.LoadFromResource("/Images/Clear.svg"))
            };

            subscriber.MessageReceived += SubscriberMessageReceived;
            subscriber.Start();
        }


        private void ClearExecute()
        {
            Messages.Clear();
            clearCommand.RaiseCanExecuteChanged();
        }


        private bool ClearCanExecute()
        {
            return Messages.Count > 0;
        }


        private void SubscriberMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            RunFromUiScheduler(() =>
            {
                Messages.Add(args.MessageInfo);
                clearCommand.RaiseCanExecuteChanged();
            });
        }


        private void UpdateSelectedMessageProperties()
        {
            SelectedMessageProperties = SelectedMessage != null
                ? MessagePropertiesRenderer.Render(SelectedMessage.Properties)
                : null;
        }


        private void RunFromUiScheduler(Action action)
        {
            _ = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
        }
    }
    
    
    public class DesignTimeSubscriberViewModel : SubscriberViewModel
    {
        public DesignTimeSubscriberViewModel() : base(new DesignTimeSubscriber())
        {
        }
        
        
        private class DesignTimeSubscriber : ISubscriber
        {
            public ValueTask DisposeAsync()
            {
                return default;
            }


            public string Exchange => "dummy";
            public string RoutingKey => "dummy";

            #pragma warning disable CS0067
            public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
            #pragma warning restore CS0067

            public void Start()
            {
            }
        }    
    }
}
