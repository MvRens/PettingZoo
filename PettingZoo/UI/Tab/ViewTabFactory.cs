using System;
using System.Collections.Generic;
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

        // Not the cleanest way, but this factory itself can't be singleton without (justifyable) upsetting SimpleInjector
        private static ISubscriber? replySubscriber;
        private static ITab? replySubscriberTab;


        public ViewTabFactory(ILogger logger, ITabHostProvider tabHostProvider, IExampleGenerator exampleGenerator, IExportImportFormatProvider exportImportFormatProvider)
        {
            this.logger = logger;
            this.tabHostProvider = tabHostProvider;
            this.exampleGenerator = exampleGenerator;
            this.exportImportFormatProvider = exportImportFormatProvider;
        }


        public void CreateSubscriberTab(IConnection? connection, ISubscriber subscriber)
        {
            InternalCreateSubscriberTab(connection, subscriber, false);
        }


        public string CreateReplySubscriberTab(IConnection connection)
        {
            if (replySubscriber?.QueueName != null && replySubscriberTab != null)
            {
                tabHostProvider.Instance.ActivateTab(replySubscriberTab);
                return replySubscriber.QueueName;
            }

            replySubscriber = new SubscriberDecorator(connection.Subscribe(), () =>
            {
                replySubscriber = null;
                replySubscriberTab = null;
            });

            replySubscriber.Start();

            replySubscriberTab = InternalCreateSubscriberTab(connection, replySubscriber, true);
            return replySubscriber.QueueName!;
        }


        public void CreatePublisherTab(IConnection connection, ReceivedMessageInfo? fromReceivedMessage = null)
        {
            var viewModel = new PublisherViewModel(this, connection, exampleGenerator, fromReceivedMessage);
            var tab = new ViewTab<PublisherView, PublisherViewModel>(
                new PublisherView(viewModel),
                viewModel,
                vm => vm.Title);

            tabHostProvider.Instance.AddTab(tab);
        }


        private ITab InternalCreateSubscriberTab(IConnection? connection, ISubscriber subscriber, bool isReplyTab)
        {
            var viewModel = new SubscriberViewModel(logger, this, connection, subscriber, exportImportFormatProvider, isReplyTab);
            var tab = new ViewTab<SubscriberView, SubscriberViewModel>(
                new SubscriberView(viewModel),
                viewModel,
                vm => vm.Title);

            tabHostProvider.Instance.AddTab(tab);
            return tab;
        }



        private class SubscriberDecorator : ISubscriber
        {
            private readonly ISubscriber decoratedSubscriber;
            private readonly Action onDispose;


            public string? QueueName => decoratedSubscriber.QueueName;
            public string? Exchange => decoratedSubscriber.Exchange;
            public string? RoutingKey => decoratedSubscriber.RoutingKey;

            public event EventHandler<MessageReceivedEventArgs>? MessageReceived;


            public SubscriberDecorator(ISubscriber decoratedSubscriber, Action onDispose)
            {
                this.decoratedSubscriber = decoratedSubscriber;
                this.onDispose = onDispose;

                decoratedSubscriber.MessageReceived += (sender, args) =>
                {
                    MessageReceived?.Invoke(sender, args);
                };
            }


            public void Dispose()
            {
                GC.SuppressFinalize(this);
                decoratedSubscriber.Dispose();
                onDispose();
            }


            public IEnumerable<ReceivedMessageInfo> GetInitialMessages()
            {
                return decoratedSubscriber.GetInitialMessages();
            }

            public void Start()
            {
                decoratedSubscriber.Start();
            }
        }
    }
}
