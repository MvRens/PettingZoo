using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Rendering;
using PettingZoo.RabbitMQ;

// TODO update title with unread message count if tab is not active

namespace PettingZoo.UI.Tab.Subscriber
{
    public class SubscriberViewModel : BaseViewModel, ITabToolbarCommands
    {
        private readonly ISubscriber subscriber;
        private readonly TaskScheduler uiScheduler;
        private MessageInfo? selectedMessage;
        private readonly DelegateCommand clearCommand;
        private readonly TabToolbarCommand[] toolbarCommands;


        public ICommand ClearCommand => clearCommand;

        public ObservableCollection<MessageInfo> Messages { get; }

        public MessageInfo? SelectedMessage
        {
            get => selectedMessage;
            set
            {
                if (value == selectedMessage)
                    return;

                selectedMessage = value;
                RaisePropertyChanged();
                RaiseOtherPropertyChanged(nameof(SelectedMessageBody));
                RaiseOtherPropertyChanged(nameof(SelectedMessageProperties));
            }
        }

        public string SelectedMessageBody =>
            SelectedMessage != null
                ? MessageBodyRenderer.Render(SelectedMessage.Body, SelectedMessage.Properties.ContentType())
                : "";

        public IDictionary<string, string>? SelectedMessageProperties => SelectedMessage?.Properties;

        public string Title => $"{subscriber.Exchange} - {subscriber.RoutingKey}";
        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        public SubscriberViewModel(ISubscriber subscriber)
        {
            this.subscriber = subscriber;
            
            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Messages = new ObservableCollection<MessageInfo>();
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


            public string Exchange { get; } = "dummy";
            public string RoutingKey { get; } = "dummy";
            
            #pragma warning disable CS0067
            public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
            #pragma warning restore CS0067

            public void Start()
            {
            }
        }    
    }
}
