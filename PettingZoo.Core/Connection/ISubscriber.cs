using System;

namespace PettingZoo.Core.Connection
{
    public interface ISubscriber : IAsyncDisposable
    {
        string Exchange {get; }
        string RoutingKey { get; }
        
        event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        void Start();
    }


    public class MessageReceivedEventArgs : EventArgs
    {
        public ReceivedMessageInfo MessageInfo { get; }


        public MessageReceivedEventArgs(ReceivedMessageInfo messageInfo)
        {
            MessageInfo = messageInfo;
        }
    }
}
