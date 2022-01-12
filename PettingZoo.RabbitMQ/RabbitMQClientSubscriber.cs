using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PettingZoo.Core.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PettingZoo.RabbitMQ
{
    public class RabbitMQClientSubscriber : ISubscriber
    {
        private IModel? model;
        
        private string? consumerTag;
        private bool started;

        public string? QueueName { get; private set; }
        public string? Exchange { get; }
        public string? RoutingKey { get; }
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;


        public RabbitMQClientSubscriber(IModel? model, string? exchange, string? routingKey)
        {
            this.model = model;
            Exchange = exchange;
            RoutingKey = routingKey;
        }


        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            if (model != null && consumerTag != null && model.IsOpen)
                model.BasicCancelNoWait(consumerTag);

            return default;
        }


        public IEnumerable<ReceivedMessageInfo> GetInitialMessages()
        {
            return Enumerable.Empty<ReceivedMessageInfo>();
        }


        public void Start()
        {
            started = true;
            if (model == null)
                return;
            
            QueueName = model.QueueDeclare().QueueName;
            if (Exchange != null && RoutingKey != null)
                model.QueueBind(QueueName, Exchange, RoutingKey);

            var consumer = new EventingBasicConsumer(model);
            consumer.Received += ClientReceived;

            consumerTag = model.BasicConsume(QueueName, true, consumer);
        }
        
        
        public void Connected(IModel newModel)
        {
            model = newModel;
            
            if (started)
                Start();
        }


        private void ClientReceived(object? sender, BasicDeliverEventArgs args)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(
                new ReceivedMessageInfo(
                    args.Exchange, 
                    args.RoutingKey, 
                    args.Body.ToArray(), 
                    RabbitMQClientPropertiesConverter.Convert(args.BasicProperties),
                    args.BasicProperties.Timestamp.UnixTime > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(args.BasicProperties.Timestamp.UnixTime).LocalDateTime
                        : DateTime.Now
                )
            ));
        }

    }
}
