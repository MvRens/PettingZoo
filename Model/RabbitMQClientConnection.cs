using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PettingZoo.Properties;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PettingZoo.Model
{
    public class RabbitMQClientConnection : IConnection
    {
        private readonly CancellationTokenSource connectionTaskToken;
        private RabbitMQ.Client.IConnection connection;
        private IModel model;


        public event EventHandler<MessageReceivedEventArgs> MessageReceived;


        public RabbitMQClientConnection(ConnectionInfo connectionInfo)
        {
            connectionTaskToken = new CancellationTokenSource();
            var connectionToken = connectionTaskToken.Token;

            Task.Factory.StartNew(() => TryConnection(connectionInfo, connectionToken), connectionToken);
        }


        public void Dispose()
        {
            connectionTaskToken.Cancel();

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


        private void TryConnection(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = connectionInfo.Host,
                Port = connectionInfo.Port,
                VirtualHost = connectionInfo.VirtualHost,
                UserName = connectionInfo.Username,
                Password = connectionInfo.Password
            };

            // ToDo exception handling
            connection = factory.CreateConnection();
            model = connection.CreateModel();

            var queueName = model.QueueDeclare().QueueName;
            model.QueueBind(queueName, connectionInfo.Exchange, connectionInfo.RoutingKey);


            var consumer = new EventingBasicConsumer(model);
            consumer.Received += ClientReceived;

            model.BasicConsume(queueName, true, consumer);
        }


        private void ClientReceived(object sender, BasicDeliverEventArgs args)
        {
            if (MessageReceived == null)
                return;

            MessageReceived(this, new MessageReceivedEventArgs(
                new MessageInfo
                {
                    Exchange = args.Exchange,
                    RoutingKey = args.RoutingKey,
                    Body = args.Body,
                    Properties = ConvertProperties(args.BasicProperties)
                }
            ));
        }


        private static Dictionary<string, string> ConvertProperties(IBasicProperties basicProperties)
        {
            var properties = new Dictionary<string, string>();

            if (basicProperties.IsDeliveryModePresent())
            {
                string deliveryMode;

                switch (basicProperties.DeliveryMode)
                {
                    case 1:
                        deliveryMode = Resources.DeliveryModeNonPersistent;
                        break;

                    case 2:
                        deliveryMode = Resources.DeliveryModePersistent;
                        break;

                    default:
                        deliveryMode = basicProperties.DeliveryMode.ToString(CultureInfo.InvariantCulture);
                        break;
                }

                properties.Add(RabbitMQProperties.DeliveryMode, deliveryMode);
            }

            if (basicProperties.IsContentTypePresent())
                properties.Add(RabbitMQProperties.ContentType, basicProperties.ContentType);

            if (basicProperties.IsContentEncodingPresent())
                properties.Add(RabbitMQProperties.ContentEncoding, basicProperties.ContentEncoding);

            if (basicProperties.IsPriorityPresent())
                properties.Add(RabbitMQProperties.Priority, basicProperties.Priority.ToString(CultureInfo.InvariantCulture));

            if (basicProperties.IsCorrelationIdPresent())
                properties.Add(RabbitMQProperties.Priority, basicProperties.CorrelationId);

            if (basicProperties.IsReplyToPresent())
                properties.Add(RabbitMQProperties.ReplyTo, basicProperties.ReplyTo);

            if (basicProperties.IsExpirationPresent())
                properties.Add(RabbitMQProperties.Expiration, basicProperties.Expiration);

            if (basicProperties.IsMessageIdPresent())
                properties.Add(RabbitMQProperties.MessageId, basicProperties.MessageId);

            if (basicProperties.IsTimestampPresent())
                properties.Add(RabbitMQProperties.Timestamp, basicProperties.Timestamp.UnixTime.ToString(CultureInfo.InvariantCulture));

            if (basicProperties.IsTypePresent())
                properties.Add(RabbitMQProperties.Type, basicProperties.Type);

            if (basicProperties.IsUserIdPresent())
                properties.Add(RabbitMQProperties.UserId, basicProperties.UserId);

            if (basicProperties.IsAppIdPresent())
                properties.Add(RabbitMQProperties.UserId, basicProperties.AppId);

            if (basicProperties.IsClusterIdPresent())
                properties.Add(RabbitMQProperties.ClusterId, basicProperties.ClusterId);
        
            foreach (var header in basicProperties.Headers)
                properties.Add(header.Key, Encoding.UTF8.GetString((byte[])header.Value));

            return properties;
        }
    }
}
