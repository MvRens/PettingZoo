using PettingZoo.Core.Connection;
using PettingZoo.Core.Generator;
using PettingZoo.UI.Tab.Publisher;
using PettingZoo.UI.Tab.Subscriber;

namespace PettingZoo.UI.Tab
{
    public class ViewTabFactory : ITabFactory
    {
        private readonly ITabHost tabHost;
        private readonly IExampleGenerator exampleGenerator;


        public ViewTabFactory(ITabHost tabHost, IExampleGenerator exampleGenerator)
        {
            this.tabHost = tabHost;
            this.exampleGenerator = exampleGenerator;
        }


        public ITab CreateSubscriberTab(IConnection connection, ISubscriber subscriber)
        {
            var viewModel = new SubscriberViewModel(tabHost, this, connection, subscriber);
            return new ViewTab<SubscriberView, SubscriberViewModel>(
                new SubscriberView(viewModel),
                viewModel,
                vm => vm.Title);
        }

        
        public ITab CreatePublisherTab(IConnection connection, ReceivedMessageInfo? fromReceivedMessage = null)
        {
            var viewModel = new PublisherViewModel(tabHost, this, connection, exampleGenerator, fromReceivedMessage);
            return new ViewTab<PublisherView, PublisherViewModel>(
                new PublisherView(viewModel),
                viewModel,
                vm => vm.Title);
        }
    }
}
