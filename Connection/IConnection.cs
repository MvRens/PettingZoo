using System;

namespace PettingZoo.Connection
{
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }


    public class StatusChangedEventArgs : EventArgs
    {
        public ConnectionStatus Status { get; private set; }
        public string Context { get; private set; }


        public StatusChangedEventArgs(ConnectionStatus status, string context)
        {
            Status = status;
            Context = context;
        }
    }


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
        event EventHandler<StatusChangedEventArgs> StatusChanged;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
