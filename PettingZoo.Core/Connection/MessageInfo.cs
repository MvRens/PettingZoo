using System;
using System.Collections.Generic;

namespace PettingZoo.Core.Connection
{
    public class BaseMessageInfo
    {
        public string Exchange { get; }
        public string RoutingKey { get; }
        public byte[] Body { get; }
        public MessageProperties Properties { get; }

        public BaseMessageInfo(string exchange, string routingKey, byte[] body, MessageProperties properties)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            Body = body;
            Properties = properties;
        }
    }


    public class ReceivedMessageInfo : BaseMessageInfo
    {
        public DateTime ReceivedTimestamp { get; }

        public ReceivedMessageInfo(string exchange, string routingKey, byte[] body, MessageProperties properties, DateTime receivedTimestamp)
            : base(exchange, routingKey, body, properties)
        {
            ReceivedTimestamp = receivedTimestamp;
        }
    }


    public class PublishMessageInfo : BaseMessageInfo
    {
        public PublishMessageInfo(string exchange, string routingKey, byte[] body, MessageProperties properties) 
            : base(exchange, routingKey, body, properties)
        {
        }
    }


    public enum MessageDeliveryMode
    {
        NonPersistent = 1,
        Persistent = 2
    }


    public class MessageProperties
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyHeaders = new Dictionary<string, string>();

        public MessageProperties(IReadOnlyDictionary<string, string>? headers)
        {
            Headers = headers ?? EmptyHeaders;
        }

        public string? AppId { get; init; }
        public string? ContentEncoding { get; init; }
        public string? ContentType { get; init; }
        public string? CorrelationId { get; init; }
        public MessageDeliveryMode? DeliveryMode { get; init; }
        public string? Expiration { get; init; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public string? MessageId { get; init; }
        public byte? Priority { get; init; }
        public string? ReplyTo { get; init; }
        public DateTime? Timestamp { get; init; }
        public string? Type { get; init; }
        public string? UserId { get; init; }
    }
}
