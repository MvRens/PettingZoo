using System;
using System.Collections.Generic;

namespace PettingZoo.Core.Connection
{
    public class MessageInfo
    {
        public DateTime Timestamp { get; }
        public string Exchange { get; }
        public string RoutingKey { get; }
        public byte[] Body { get; }
        public IDictionary<string, string> Properties { get; }

        public MessageInfo(string exchange, string routingKey, byte[] body, IDictionary<string, string> properties, DateTime timestamp)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            Body = body;
            Properties = properties;
            Timestamp = timestamp;
        }
    }
}
