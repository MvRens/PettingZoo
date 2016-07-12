using System;
using System.Collections.Generic;

namespace PettingZoo.Model
{
    public class MessageInfo
    {
        public DateTime Timestamp { get; set;  }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public byte[] Body { get; set; }

        public Dictionary<string, string> Properties;

        public string ContentType
        {
            get
            {
                return Properties != null && Properties.ContainsKey(RabbitMQProperties.ContentType)
                    ? Properties[RabbitMQProperties.ContentType]
                    : "";
            }
        }


        public MessageInfo()
        {
            Timestamp = DateTime.Now;
        }
    }
}
