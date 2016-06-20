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


        public static ConnectionInfo Default()
        {
            return new ConnectionInfo()
            {
                Host = "localhost",
                Port = 5672,
                VirtualHost = "/",
                Username = "guest",
                Password = "guest",
                RoutingKey = "#"
            };
        }
    }
}
