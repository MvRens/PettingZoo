using System;

namespace PettingZoo.UI.Connection
{
    public interface IConnectionDialog
    {
        ConnectionDialogParams? Show(ConnectionDialogParams? defaultParams = null);
    }


    public class ConnectionDialogParams
    {
        public string Host { get; }
        public string VirtualHost { get; }
        public int Port { get; }
        public string Username { get; }
        public string Password { get; }

        public bool Subscribe { get; }
        public string Exchange { get; }
        public string RoutingKey { get; }


        public static ConnectionDialogParams Default { get; } = new("localhost", "/", 5672, "guest", "guest", false, "", "#");


        public ConnectionDialogParams(string host, string virtualHost, int port, string username, string password, bool subscribe, string exchange, string routingKey)
        {
            if (subscribe && (string.IsNullOrEmpty(exchange) || string.IsNullOrEmpty(routingKey)))
                throw new ArgumentException(@"Exchange and RoutingKey must be provided when Subscribe is true", nameof(subscribe));
            
            Host = host;
            VirtualHost = virtualHost;
            Port = port;
            Username = username;
            Password = password;
            
            Subscribe = subscribe;
            Exchange = exchange;
            RoutingKey = routingKey;
        }
    }
}
