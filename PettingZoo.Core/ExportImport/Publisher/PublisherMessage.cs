using System.Collections.Generic;
using PettingZoo.Core.Connection;

namespace PettingZoo.Core.ExportImport.Publisher
{
    public enum PublisherMessageType
    {
        Raw,
        Tapeti
    }



    public class PublisherMessage
    {
        public PublisherMessageType MessageType { get; set; }
        public bool SendToExchange { get; set; }
        public string? Exchange { get; set; }
        public string? RoutingKey { get; set; }
        public string? Queue { get; set; }
        public bool ReplyToNewSubscriber { get; set; }
        public string? ReplyTo { get; set; }

        public RawPublisherMessage? RawPublisherMessage { get; set; }
        public TapetiPublisherMessage? TapetiPublisherMessage { get; set; }
    }


    public class RawPublisherMessage
    {
        public MessageDeliveryMode DeliveryMode { get; set; }

        public string? ContentType { get; set; }
        public string? CorrelationId { get; set; }
        public string? AppId { get; set; }
        public string? ContentEncoding { get; set; }
        public string? Expiration { get; set; }
        public string? MessageId { get; set; }
        public string? Priority { get; set; }
        public string? Timestamp { get; set; }
        public string? TypeProperty { get; set; }
        public string? UserId { get; set; }
        public string? Payload { get; set; }
        public bool EnableMacros { get; set; }

        public Dictionary<string, string>? Headers { get; set; }
    }


    public class TapetiPublisherMessage
    {
        public string? CorrelationId { get; set; }
        public string? Payload { get; set; }
        public bool EnableMacros { get; set; }
        public string? ClassName { get; set; }
        public string? AssemblyName { get; set; }
    }
}
