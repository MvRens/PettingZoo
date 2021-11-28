using System.Collections.Generic;

namespace PettingZoo.RabbitMQ
{
    public static class RabbitMQPropertiesExtensions
    {
        public static string ContentType(this IDictionary<string, string> properties)
        {
            return properties.TryGetValue(RabbitMQProperties.ContentType, out var value)
                ? value
                : "";
        }
    }
}
