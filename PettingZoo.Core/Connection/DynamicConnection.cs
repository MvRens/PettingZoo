using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PettingZoo.Core.Connection
{
    public class DynamicConnection : IConnection
    {
        public Guid ConnectionId => currentConnection?.ConnectionId ?? Guid.Empty;
        public ConnectionParams? ConnectionParams { get; private set; }
        public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;
        public event EventHandler<StatusChangedEventArgs>? StatusChanged;


        private IConnection? currentConnection;


        public async ValueTask DisposeAsync()
        {
            if (currentConnection != null)
                await currentConnection.DisposeAsync();

            GC.SuppressFinalize(this);
        }


        public void Connect()
        {
            CheckConnection();
            currentConnection.Connect();

        }

        public async ValueTask Disconnect()
        {
            if (currentConnection == null)
                return;

            var disconnectedConnectionId = currentConnection.ConnectionId;
            await currentConnection.DisposeAsync();
            currentConnection = null;

            ConnectionStatusChanged(this, new StatusChangedEventArgs(disconnectedConnectionId, ConnectionStatus.Disconnected));
        }


        public void SetConnection(IConnection connection)
        {
            if (currentConnection != null)
            {
                currentConnection.StatusChanged -= ConnectionStatusChanged;
                ConnectionStatusChanged(this, new StatusChangedEventArgs(currentConnection.ConnectionId, ConnectionStatus.Disconnected));
            }

            currentConnection = connection;

            // Assume we get the new connection before Connect is called, thus before the status changes
            if (currentConnection != null)
                currentConnection.StatusChanged += ConnectionStatusChanged;
        }


        public ISubscriber Subscribe(string exchange, string routingKey)
        {
            CheckConnection();
            return currentConnection.Subscribe(exchange, routingKey);
        }


        public ISubscriber Subscribe()
        {
            CheckConnection();
            return currentConnection.Subscribe();
        }


        public Task Publish(PublishMessageInfo messageInfo)
        {
            CheckConnection();
            return currentConnection.Publish(messageInfo);
        }


        private void ConnectionStatusChanged(object? sender, StatusChangedEventArgs e)
        {
            ConnectionParams = e.ConnectionParams;
            Status = e.Status;

            StatusChanged?.Invoke(sender, e);
        }


        [MemberNotNull(nameof(currentConnection))]
        private void CheckConnection()
        {
            if (currentConnection == null)
                throw new InvalidOperationException("No current connection");
        }
    }
}
