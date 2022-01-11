using PettingZoo.Core.Connection;
using PettingZoo.Core.ExportImport;
using PettingZoo.Core.Generator;
using PettingZoo.UI.Tab.Publisher;
using PettingZoo.UI.Tab.Subscriber;
using Serilog;

namespace PettingZoo.UI.Tab
{
    public class ViewTabFactory : ITabFactory
    {
        private readonly ILogger logger;
        private readonly ITabHostProvider tabHostProvider;
        private readonly IExampleGenerator exampleGenerator;
        private readonly IExportImportFormatProvider exportImportFormatProvider;


        public ViewTabFactory(ILogger logger, ITabHostProvider tabHostProvider, IExampleGenerator exampleGenerator, IExportImportFormatProvider exportImportFormatProvider)
        {
            this.logger = logger;
            this.tabHostProvider = tabHostProvider;
            this.exampleGenerator = exampleGenerator;
            this.exportImportFormatProvider = exportImportFormatProvider;
        }


        public ITab CreateSubscriberTab(IConnection? connection, ISubscriber subscriber)
        {
            var viewModel = new SubscriberViewModel(logger, tabHostProvider, this, connection, subscriber, exportImportFormatProvider);
            return new ViewTab<SubscriberView, SubscriberViewModel>(
                new SubscriberView(viewModel),
                viewModel,
                vm => vm.Title);
        }

        
        public ITab CreatePublisherTab(IConnection connection, ReceivedMessageInfo? fromReceivedMessage = null)
        {
            var viewModel = new PublisherViewModel(tabHostProvider, this, connection, exampleGenerator, fromReceivedMessage);
            return new ViewTab<PublisherView, PublisherViewModel>(
                new PublisherView(viewModel),
                viewModel,
                vm => vm.Title);
        }
    }
}
