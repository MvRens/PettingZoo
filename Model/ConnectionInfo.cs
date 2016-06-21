namespace PettingZoo.Model
{
    public class ConnectionInfo
    {
        public string Host { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string Exchange { get; set; }
        public string RoutingKey { get; set;  }
    }
}
