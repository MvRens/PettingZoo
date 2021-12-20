using PettingZoo.Core.Connection;

namespace PettingZoo.UI.Tab
{
    // Passing the closeTabCommand is necessary because I haven't figured out how to bind the main window's
    // context menu items for the tab to the main window's datacontext yet. RelativeSource doesn't seem to work
    // because the popup is it's own window. Refactor if a better solution is found.
    
    public interface ITabFactory
    {
        ITab CreateSubscriberTab(IConnection connection, ISubscriber subscriber);
        ITab CreatePublisherTab(IConnection connection, ReceivedMessageInfo? fromReceivedMessage = null);
    }
}
