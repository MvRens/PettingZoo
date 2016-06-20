using System.Collections.Generic;

namespace PettingZoo.Model
{
    public class MessageInfo
    {
        public string RoutingKey { get; set; }
        public byte[] Body { get; set; }

        public Dictionary<string, string> Properties;

        public string ContentType
        {
            get
            {
                return Properties != null && Properties.ContainsKey("content-type")
                    ? Properties["content-type"]
                    : "";
            }
        }
    }
}
