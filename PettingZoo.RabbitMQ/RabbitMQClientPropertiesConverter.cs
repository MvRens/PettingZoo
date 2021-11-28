using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RabbitMQ.Client;

namespace PettingZoo.RabbitMQ
{
    public static class RabbitMQClientPropertiesConverter
    {
        public static IDictionary<string, string> Convert(IBasicProperties basicProperties)
        {
            var properties = new Dictionary<string, string>();

            if (basicProperties.IsDeliveryModePresent())
                properties.Add(RabbitMQProperties.DeliveryMode, basicProperties.DeliveryMode.ToString(CultureInfo.InvariantCulture));

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

            // ReSharper disable once InvertIf
            if (basicProperties.Headers != null)
            {
                foreach (var (key, value) in basicProperties.Headers)
                    properties.Add(key, Encoding.UTF8.GetString((byte[]) value));
            }

            return properties;
        }


        public static IBasicProperties Convert(IDictionary<string, string> properties, IBasicProperties targetProperties)
        {
            foreach (var (key, value) in properties)
            {
                switch (key)
                {
                    case RabbitMQProperties.DeliveryMode:
                        if (byte.TryParse(value, out var deliveryMode))
                            targetProperties.DeliveryMode = deliveryMode;

                        break;

                    case RabbitMQProperties.ContentType:
                        targetProperties.ContentType = value;
                        break;

                    case RabbitMQProperties.ContentEncoding:
                        targetProperties.ContentEncoding = value;
                        break;

                    case RabbitMQProperties.Priority:
                        if (byte.TryParse(value, out var priority))
                            targetProperties.Priority = priority;
                        
                        break;

                    case RabbitMQProperties.CorrelationId:
                        targetProperties.CorrelationId = value;
                        break;
                    
                    case RabbitMQProperties.ReplyTo:
                        targetProperties.ReplyTo = value;
                        break;

                    case RabbitMQProperties.Expiration:
                        targetProperties.Expiration = value;
                        break;

                    case RabbitMQProperties.MessageId:
                        targetProperties.MessageId = value;
                        break;

                    case RabbitMQProperties.Timestamp:
                        if (long.TryParse(value, out var timestamp))
                            targetProperties.Timestamp = new AmqpTimestamp(timestamp);
                        
                        break;

                    case RabbitMQProperties.Type:
                        targetProperties.Type = value;
                        break;

                    case RabbitMQProperties.UserId:
                        targetProperties.UserId = value;
                        break;

                    case RabbitMQProperties.AppId:
                        targetProperties.AppId = value;
                        break;

                    case RabbitMQProperties.ClusterId:
                        targetProperties.ClusterId = value;
                        break;

                    default:
                        targetProperties.Headers ??= new Dictionary<string, object>();
                        targetProperties.Headers.Add(key, Encoding.UTF8.GetBytes(value));
                        break;
                }
            }
            
            return targetProperties;
        }
    }
}
