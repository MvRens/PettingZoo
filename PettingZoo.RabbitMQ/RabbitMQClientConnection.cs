using System;
using System.Threading;
using System.Threading.Tasks;
using PettingZoo.Core.Connection;
using RabbitMQ.Client;
using IConnection = RabbitMQ.Client.IConnection;

namespace PettingZoo.RabbitMQ
{
    public class RabbitMQClientConnection : Core.Connection.IConnection
    {
        public Guid ConnectionId { get; } = Guid.NewGuid();
        public ConnectionParams? ConnectionParams { get; }
        public ConnectionStatus Status { get; set; }
        public event EventHandler<StatusChangedEventArgs>? StatusChanged;

        
        private const int ConnectRetryDelay = 5000;

        private readonly CancellationTokenSource connectionTaskToken = new();
        private Task? connectionTask;
        private readonly object connectionLock = new();
        private IConnection? connection;


        public RabbitMQClientConnection(ConnectionParams connectionParams)
        {
            ConnectionParams = connectionParams;
        }


        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            if (connectionTask == null)
                return;

            connectionTaskToken.Cancel();
            if (!connectionTask.IsCompleted)
                await connectionTask;

            lock (connectionLock)
            {
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
            }
        }


        public void Connect()
        {
            connectionTask = Task.Factory.StartNew(() => TryConnection(ConnectionParams!, connectionTaskToken.Token), CancellationToken.None);
        }


        public ISubscriber Subscribe(string exchange, string routingKey)
        {
            return CreateSubscriber(exchange, routingKey);
        }


        public ISubscriber Subscribe()
        {
            return CreateSubscriber(null, null);
        }


        private ISubscriber CreateSubscriber(string? exchange, string? routingKey)
        {
            lock (connectionLock)
            {
                var model = connection?.CreateModel();
                var subscriber = new RabbitMQClientSubscriber(model, exchange, routingKey);
                if (model != null)
                    return subscriber;


                void ConnectSubscriber(object? sender, StatusChangedEventArgs args)
                {
                    if (args.Status != ConnectionStatus.Connected)
                        return;

                    lock (connectionLock)
                    {
                        if (connection == null)
                            return;

                        subscriber.Connected(connection.CreateModel());
                    }

                    StatusChanged -= ConnectSubscriber;
                }


                StatusChanged += ConnectSubscriber;
                return subscriber;
            }
        }


        public Task Publish(PublishMessageInfo messageInfo)
        {
            IConnection? lockedConnection;

            lock (connectionLock)
            {
                lockedConnection = connection;
            }

            if (lockedConnection == null)
                throw new InvalidOperationException("Not connected");

            using (var model = lockedConnection.CreateModel())
            {
                try
                {
                    model.BasicPublish(messageInfo.Exchange, messageInfo.RoutingKey, false,
                        RabbitMQClientPropertiesConverter.Convert(messageInfo.Properties,
                            model.CreateBasicProperties()),
                        messageInfo.Body);
                }
                finally
                {
                    model.Close();
                }
            }

            return Task.CompletedTask;
        }

        
        private void TryConnection(ConnectionParams connectionParams, CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = connectionParams.Host,
                Port = connectionParams.Port,
                VirtualHost = connectionParams.VirtualHost,
                UserName = connectionParams.Username,
                Password = connectionParams.Password
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                DoStatusChanged(ConnectionStatus.Connecting);
                try
                {
                    lock (connectionLock)
                    {
                        connection = factory.CreateConnection();
                    }

                    DoStatusChanged(ConnectionStatus.Connected);
                    break;
                }
                catch (Exception e)
                {
                    DoStatusChanged(ConnectionStatus.Error, e);

                    try
                    {
                        Task.Delay(ConnectRetryDelay, cancellationToken).Wait(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
        }


        private void DoStatusChanged(ConnectionStatus status, Exception? exception = null)
        {
            Status = status;
            StatusChanged?.Invoke(this, new StatusChangedEventArgs(ConnectionId, status, ConnectionParams, exception));
        }
    }
}
