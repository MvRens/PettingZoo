using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.UI.Tab.Publisher;
using PettingZoo.UI.Tab.Subscriber;

namespace PettingZoo.UI.Tab
{
    public class ViewTabFactory : ITabFactory
    {
        private readonly ITabHost tabHost;
        private readonly ICommand closeTabCommand;


        public ViewTabFactory(ITabHost tabHost, ICommand closeTabCommand)
        {
            this.tabHost = tabHost;
            this.closeTabCommand = closeTabCommand;
        }


        public ITab CreateSubscriberTab(IConnection connection, ISubscriber subscriber)
        {
            var viewModel = new SubscriberViewModel(tabHost, this, connection, subscriber);
            return new ViewTab<SubscriberView, SubscriberViewModel>(
                closeTabCommand,
                new SubscriberView(viewModel),
                viewModel,
                vm => vm.Title);
        }

        
        public ITab CreatePublisherTab(IConnection connection, ReceivedMessageInfo? fromReceivedMessage = null)
        {
            var viewModel = new PublisherViewModel(connection, fromReceivedMessage);
            return new ViewTab<PublisherView, PublisherViewModel>(
                closeTabCommand,
                new PublisherView(viewModel),
                viewModel,
                vm => vm.Title);
        }
    }
}
