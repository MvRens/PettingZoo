using System;
using System.Collections.Generic;
using System.Linq;
using PettingZoo.Core.Connection;

namespace PettingZoo.Core.ExportImport.Publisher
{
    public enum PublisherMessageType
    {
        Raw,
        Tapeti
    }



    public class PublisherMessage : IEquatable<PublisherMessage>
    {
        public PublisherMessageType MessageType { get; init; }
        public bool SendToExchange { get; init; }
        public string? Exchange { get; init; }
        public string? RoutingKey { get; init; }
        public string? Queue { get; init; }
        public bool ReplyToNewSubscriber { get; init; }
        public string? ReplyTo { get; init; }

        public RawPublisherMessage? RawPublisherMessage { get; init; }
        public TapetiPublisherMessage? TapetiPublisherMessage { get; init; }


        public bool Equals(PublisherMessage? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            return MessageType == other.MessageType && 
                   SendToExchange == other.SendToExchange && 
                   Exchange == other.Exchange && 
                   RoutingKey == other.RoutingKey && 
                   Queue == other.Queue && 
                   ReplyToNewSubscriber == other.ReplyToNewSubscriber && 
                   ReplyTo == other.ReplyTo && 
                   Equals(RawPublisherMessage, other.RawPublisherMessage) && 
                   Equals(TapetiPublisherMessage, other.TapetiPublisherMessage);
        }


        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is PublisherMessage publisherMessage && Equals(publisherMessage);
        }


        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add((int)MessageType);
            hashCode.Add(SendToExchange);
            hashCode.Add(Exchange);
            hashCode.Add(RoutingKey);
            hashCode.Add(Queue);
            hashCode.Add(ReplyToNewSubscriber);
            hashCode.Add(ReplyTo);
            hashCode.Add(RawPublisherMessage);
            hashCode.Add(TapetiPublisherMessage);
            return hashCode.ToHashCode();
        }
    }


    public class RawPublisherMessage : IEquatable<RawPublisherMessage>
    {
        public MessageDeliveryMode DeliveryMode { get; init; }

        public string? ContentType { get; init; }
        public string? CorrelationId { get; init; }
        public string? AppId { get; init; }
        public string? ContentEncoding { get; init; }
        public string? Expiration { get; init; }
        public string? MessageId { get; init; }
        public string? Priority { get; init; }
        public string? Timestamp { get; init; }
        public string? TypeProperty { get; init; }
        public string? UserId { get; init; }
        public string? Payload { get; init; }
        public bool EnableMacros { get; init; }

        public Dictionary<string, string>? Headers { get; init; }


        public bool Equals(RawPublisherMessage? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            return DeliveryMode == other.DeliveryMode && 
                   ContentType == other.ContentType && 
                   CorrelationId == other.CorrelationId && 
                   AppId == other.AppId && 
                   ContentEncoding == other.ContentEncoding && 
                   Expiration == other.Expiration && 
                   MessageId == other.MessageId && 
                   Priority == other.Priority && 
                   Timestamp == other.Timestamp && 
                   TypeProperty == other.TypeProperty && 
                   UserId == other.UserId && 
                   Payload == other.Payload && 
                   EnableMacros == other.EnableMacros && 
                   HeadersEquals(other.Headers);
        }


        private bool HeadersEquals(Dictionary<string, string>? other)
        {
            if (other == null)
                return Headers == null || Headers.Count == 0;

            if (Headers == null)
                return other.Count == 0;

            return other.OrderBy(h => h.Key).SequenceEqual(Headers.OrderBy(h => h.Key));
        }


        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is RawPublisherMessage rawPublisherMessage && Equals(rawPublisherMessage);
        }


        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add((int)DeliveryMode);
            hashCode.Add(ContentType);
            hashCode.Add(CorrelationId);
            hashCode.Add(AppId);
            hashCode.Add(ContentEncoding);
            hashCode.Add(Expiration);
            hashCode.Add(MessageId);
            hashCode.Add(Priority);
            hashCode.Add(Timestamp);
            hashCode.Add(TypeProperty);
            hashCode.Add(UserId);
            hashCode.Add(Payload);
            hashCode.Add(EnableMacros);

            if (Headers != null)
                foreach (var (key, value) in Headers)
                {
                    hashCode.Add(key);
                    hashCode.Add(value);
                }
            
            return hashCode.ToHashCode();
        }
    }


    public class TapetiPublisherMessage : IEquatable<TapetiPublisherMessage>
    {
        public string? CorrelationId { get; init; }
        public string? Payload { get; init; }
        public bool EnableMacros { get; init; }
        public string? ClassName { get; init; }
        public string? AssemblyName { get; init; }


        public bool Equals(TapetiPublisherMessage? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            return CorrelationId == other.CorrelationId && 
                   Payload == other.Payload && 
                   EnableMacros == other.EnableMacros && 
                   ClassName == other.ClassName && 
                   AssemblyName == other.AssemblyName;
        }


        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is TapetiPublisherMessage tapetiPublisherMessage && Equals(tapetiPublisherMessage);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(CorrelationId, Payload, EnableMacros, ClassName, AssemblyName);
        }
    }
}
