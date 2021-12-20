namespace PettingZoo.Core.Connection
{
    public class ConnectionParams
    {
        public string Host { get; }
        public string VirtualHost { get; }
        public int Port { get; }
        public string Username { get; }
        public string Password { get; }


        public ConnectionParams(string host, string virtualHost, int port, string username, string password)
        {
            Host = host;
            VirtualHost = virtualHost;
            Port = port;
            Username = username;
            Password = password;
        }
    }
}
