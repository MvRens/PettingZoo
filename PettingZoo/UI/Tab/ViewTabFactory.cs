using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.UI.Tab.Publisher;
using PettingZoo.UI.Tab.Subscriber;

namespace PettingZoo.UI.Tab
{
    public class ViewTabFactory : ITabFactory
    {
        public ITab CreateSubscriberTab(ICommand closeTabCommand, ISubscriber subscriber)
        {
            var viewModel = new SubscriberViewModel(subscriber);
            return new ViewTab<SubscriberView, SubscriberViewModel>(
                closeTabCommand,
                new SubscriberView(viewModel),
                viewModel,
                vm => vm.Title);
        }

        
        public ITab CreatePublisherTab(ICommand closeTabCommand, IConnection connection)
        {
            var viewModel = new PublisherViewModel(connection);
            return new ViewTab<PublisherView, PublisherViewModel>(
                closeTabCommand,
                new PublisherView(viewModel),
                viewModel,
                vm => vm.Title);
        }
    }
}
