using System;
using System.Linq;
using System.Text;
using PettingZoo.Core.Connection;
using RabbitMQ.Client;

namespace PettingZoo.RabbitMQ
{
    public static class RabbitMQClientPropertiesConverter
    {
        public static MessageProperties Convert(IBasicProperties basicProperties)
        {
            return new MessageProperties(basicProperties.Headers?.ToDictionary(p => p.Key, p => Encoding.UTF8.GetString((byte[])p.Value)))
            {
                DeliveryMode = basicProperties.IsDeliveryModePresent()
                    ? basicProperties.DeliveryMode == 2 ? MessageDeliveryMode.Persistent :
                    MessageDeliveryMode.NonPersistent
                    : null,

                ContentType = basicProperties.IsContentTypePresent()
                    ? basicProperties.ContentType
                    : null,

                ContentEncoding = basicProperties.IsContentEncodingPresent()
                    ? basicProperties.ContentEncoding
                    : null,

                Priority = basicProperties.IsPriorityPresent()
                    ? basicProperties.Priority
                    : null,

                CorrelationId = basicProperties.IsCorrelationIdPresent()
                    ? basicProperties.CorrelationId
                    : null,

                ReplyTo = basicProperties.IsReplyToPresent()
                    ? basicProperties.ReplyTo
                    : null,

                Expiration = basicProperties.IsExpirationPresent()
                    ? basicProperties.Expiration
                    : null,

                MessageId = basicProperties.IsMessageIdPresent()
                    ? basicProperties.MessageId
                    : null,

                Timestamp = basicProperties.IsTimestampPresent()
                    ? DateTimeOffset.FromUnixTimeMilliseconds(basicProperties.Timestamp.UnixTime).LocalDateTime
                    : null,

                Type = basicProperties.IsTypePresent()
                    ? basicProperties.Type
                    : null,

                UserId = basicProperties.IsUserIdPresent()
                    ? basicProperties.UserId
                    : null,

                AppId = basicProperties.IsAppIdPresent()
                    ? basicProperties.AppId
                    : null
            };
        }


        public static IBasicProperties Convert(MessageProperties properties, IBasicProperties targetProperties)
        {
            if (properties.DeliveryMode != null)
                targetProperties.DeliveryMode = properties.DeliveryMode == MessageDeliveryMode.Persistent ? (byte)2 : (byte)1;
            else
                targetProperties.ClearDeliveryMode();

            if (properties.ContentType != null)
                targetProperties.ContentType = properties.ContentType;
            else
                targetProperties.ClearContentType();

            if (properties.ContentEncoding != null)
                targetProperties.ContentEncoding = properties.ContentEncoding;
            else
                targetProperties.ClearContentEncoding();

            if (properties.Priority != null)
                targetProperties.Priority = properties.Priority.Value;
            else
                targetProperties.ClearPriority();

            if (properties.CorrelationId != null)
                targetProperties.CorrelationId = properties.CorrelationId;
            else
                targetProperties.ClearCorrelationId();

            if (properties.ReplyTo != null)
                targetProperties.ReplyTo = properties.ReplyTo;
            else
                targetProperties.ClearReplyTo();

            if (properties.Expiration != null)
                targetProperties.Expiration = properties.Expiration;
            else
                targetProperties.ClearExpiration();

            if (properties.MessageId != null)
                targetProperties.MessageId = properties.MessageId;
            else
                targetProperties.ClearMessageId();

            if (properties.Timestamp != null)
                targetProperties.Timestamp = new AmqpTimestamp(new DateTimeOffset(properties.Timestamp.Value).ToUnixTimeMilliseconds());
            else
                targetProperties.ClearTimestamp();

            if (properties.Type != null)
                targetProperties.Type = properties.Type;
            else
                targetProperties.ClearType();

            if (properties.UserId != null)
                targetProperties.UserId = properties.UserId;
            else
                targetProperties.ClearUserId();

            if (properties.AppId != null)
                targetProperties.AppId = properties.AppId;
            else
                targetProperties.ClearAppId();

            if (properties.Headers.Count > 0)
                targetProperties.Headers = properties.Headers.ToDictionary(p => p.Key, p => (object)Encoding.UTF8.GetBytes(p.Value));
            else
                targetProperties.ClearHeaders();
            
            return targetProperties;
        }
    }
}
