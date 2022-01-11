using System;
using System.Collections.Generic;

namespace PettingZoo.Core.Connection
{
    public interface ISubscriber : IAsyncDisposable
    {
        string? QueueName { get; }
        string? Exchange {get; }
        string? RoutingKey { get; }
        
        event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        IEnumerable<ReceivedMessageInfo> GetInitialMessages();
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
