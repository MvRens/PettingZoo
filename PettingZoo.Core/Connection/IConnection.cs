using System;
using System.Threading.Tasks;

namespace PettingZoo.Core.Connection
{
    public interface IConnection : IAsyncDisposable
    {
        Guid ConnectionId { get; }
        ConnectionParams? ConnectionParams { get; }
        ConnectionStatus Status { get; }

        event EventHandler<StatusChangedEventArgs> StatusChanged;


        void Connect();

        ISubscriber Subscribe(string exchange, string routingKey);
        ISubscriber Subscribe();

        Task Publish(PublishMessageInfo messageInfo);
    }


    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }


    public class StatusChangedEventArgs : EventArgs
    {
        public Guid ConnectionId { get; }
        public ConnectionStatus Status { get; }
        public ConnectionParams? ConnectionParams { get; }
        public Exception? Exception { get; }


        public StatusChangedEventArgs(Guid connectionId, ConnectionStatus status, ConnectionParams? connectionParams = null, Exception? exception = null)
        {
            ConnectionId = connectionId;
            Status = status;
            ConnectionParams = connectionParams;
            Exception = exception;
        }
    }
}
