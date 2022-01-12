using PettingZoo.Core.Connection;

namespace PettingZoo.UI.Tab
{
    public interface ITabFactory
    {
        void CreateSubscriberTab(IConnection? connection, ISubscriber subscriber);
        string CreateReplySubscriberTab(IConnection connection);
        void CreatePublisherTab(IConnection connection, ReceivedMessageInfo? fromReceivedMessage = null);
    }
}
