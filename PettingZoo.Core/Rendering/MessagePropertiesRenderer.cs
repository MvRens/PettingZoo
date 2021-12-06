using System.Collections.Generic;
using PettingZoo.Core.Connection;

namespace PettingZoo.Core.Rendering
{
    public class MessagePropertiesRenderer
    {
        public static IDictionary<string, string> Render(MessageProperties properties)
        {
            var result = new Dictionary<string, string>();

            if (properties.AppId != null)
                result.Add(MessagePropertiesRendererStrings.AppId, properties.AppId);

            if (properties.ContentEncoding != null)
                result.Add(MessagePropertiesRendererStrings.ContentEncoding, properties.ContentEncoding);

            if (properties.ContentType != null)
                result.Add(MessagePropertiesRendererStrings.ContentType, properties.ContentType);

            if (properties.CorrelationId != null)
                result.Add(MessagePropertiesRendererStrings.CorrelationId, properties.CorrelationId);

            if (properties.DeliveryMode != null)
                result.Add(MessagePropertiesRendererStrings.DeliveryMode, 
                    properties.DeliveryMode == MessageDeliveryMode.Persistent 
                        ? MessagePropertiesRendererStrings.DeliveryModePersistent
                        : MessagePropertiesRendererStrings.DeliveryModeNonPersistent);

            if (properties.Expiration != null)
                result.Add(MessagePropertiesRendererStrings.Expiration, properties.Expiration);

            if (properties.MessageId != null)
                result.Add(MessagePropertiesRendererStrings.MessageId, properties.MessageId);

            if (properties.Priority != null)
                result.Add(MessagePropertiesRendererStrings.Priority, properties.Priority.Value.ToString());

            if (properties.ReplyTo != null)
                result.Add(MessagePropertiesRendererStrings.ReplyTo, properties.ReplyTo);

            if (properties.Timestamp != null)
                result.Add(MessagePropertiesRendererStrings.Timestamp, properties.Timestamp.Value.ToString("G"));

            if (properties.Type != null)
                result.Add(MessagePropertiesRendererStrings.Type, properties.Type);

            if (properties.UserId != null)
                result.Add(MessagePropertiesRendererStrings.UserId, properties.UserId);

            foreach (var (key, value) in properties.Headers)
            {
                if (!result.TryAdd(key, value))
                    result.TryAdd(MessagePropertiesRendererStrings.HeaderPrefix + key, value);
            }

            return result;
        }
    }
}
