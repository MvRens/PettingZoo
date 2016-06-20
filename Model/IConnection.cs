using System;

namespace PettingZoo.Model
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageInfo MessageInfo { get; private set; }


        public MessageReceivedEventArgs(MessageInfo messageInfo)
        {
            MessageInfo = messageInfo;
        }
    }


    public interface IConnection : IDisposable
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
