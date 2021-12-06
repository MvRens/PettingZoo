using System;
using System.Threading;
using System.Threading.Tasks;
using PettingZoo.Core.Connection;
using RabbitMQ.Client;

namespace PettingZoo.RabbitMQ
{
    public class RabbitMQClientConnection : Core.Connection.IConnection
    {
        private const int ConnectRetryDelay = 5000;

        private readonly CancellationTokenSource connectionTaskToken = new();
        private readonly Task connectionTask;
        private readonly object connectionLock = new();
        private global::RabbitMQ.Client.IConnection? connection;
        private IModel? model;


        public event EventHandler<StatusChangedEventArgs>? StatusChanged;
        

        public RabbitMQClientConnection(ConnectionParams connectionParams)
        {
            connectionTask = Task.Factory.StartNew(() => TryConnection(connectionParams, connectionTaskToken.Token), CancellationToken.None);
        }


        public async ValueTask DisposeAsync()
        {
            connectionTaskToken.Cancel();
            if (!connectionTask.IsCompleted)
                await connectionTask;

            lock (connectionLock)
            {
                if (model != null)
                {
                    model.Dispose();
                    model = null;
                }

                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
            }
        }


        public ISubscriber Subscribe(string exchange, string routingKey)
        {
            lock (connectionLock)
            {
                var subscriber = new RabbitMQClientSubscriber(model, exchange, routingKey);
                if (model != null) 
                    return subscriber;

                
                void ConnectSubscriber(object? sender, StatusChangedEventArgs args)
                {
                    if (args.Status != ConnectionStatus.Connected)
                        return;

                    lock (connectionLock)
                    {
                        if (model == null)
                            return;
                            
                        subscriber.Connected(model);
                    }
                        
                    StatusChanged -= ConnectSubscriber;
                }

                
                StatusChanged += ConnectSubscriber;
                return subscriber;
            }
        }

        
        public Task Publish(PublishMessageInfo messageInfo)
        {
            if (model == null)
                throw new InvalidOperationException("Not connected");

            model.BasicPublish(messageInfo.Exchange, messageInfo.RoutingKey, false,
                RabbitMQClientPropertiesConverter.Convert(messageInfo.Properties, model.CreateBasicProperties()),
                messageInfo.Body);

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

            var statusContext = $"{connectionParams.Host}:{connectionParams.Port}{connectionParams.VirtualHost}";

            while (!cancellationToken.IsCancellationRequested)
            {
                DoStatusChanged(ConnectionStatus.Connecting, statusContext);
                try
                {
                    connection = factory.CreateConnection();
                    model = connection.CreateModel();
                    
                    DoStatusChanged(ConnectionStatus.Connected, statusContext);
                    break;
                }
                catch (Exception e)
                {
                    DoStatusChanged(ConnectionStatus.Error, e.Message);

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


        private void DoStatusChanged(ConnectionStatus status, string? context = null)
        {
            StatusChanged?.Invoke(this, new StatusChangedEventArgs(status, context));
        }
    }
}
