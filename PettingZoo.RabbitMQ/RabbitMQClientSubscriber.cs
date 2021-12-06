using System;
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

        public string Exchange { get; }
        public string RoutingKey { get; }
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;


        public RabbitMQClientSubscriber(IModel? model, string exchange, string routingKey)
        {
            this.model = model;
            Exchange = exchange;
            RoutingKey = routingKey;
        }


        public ValueTask DisposeAsync()
        {
            if (model != null && consumerTag != null && model.IsOpen)
                model.BasicCancelNoWait(consumerTag);

            return default;
        }


        public void Start()
        {
            started = true;
            if (model == null)
                return;
            
            var queueName = model.QueueDeclare().QueueName;
            model.QueueBind(queueName, Exchange, RoutingKey);

            var consumer = new EventingBasicConsumer(model);
            consumer.Received += ClientReceived;

            consumerTag = model.BasicConsume(queueName, true, consumer);
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
