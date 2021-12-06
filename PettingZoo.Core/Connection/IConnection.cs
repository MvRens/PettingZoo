using System;
using System.Threading.Tasks;

namespace PettingZoo.Core.Connection
{
    public interface IConnection : IAsyncDisposable
    {
        event EventHandler<StatusChangedEventArgs> StatusChanged;

        ISubscriber Subscribe(string exchange, string routingKey);
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
        public ConnectionStatus Status { get; }
        public string? Context { get; }


        public StatusChangedEventArgs(ConnectionStatus status, string? context)
        {
            Status = status;
            Context = context;
        }
    }
}
