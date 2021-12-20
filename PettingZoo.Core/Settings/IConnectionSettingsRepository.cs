using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PettingZoo.Core.Settings
{
    public interface IConnectionSettingsRepository
    {
        Task<StoredConnectionSettings> GetLastUsed();
        Task StoreLastUsed(bool storePassword, ConnectionSettings connectionSettings);

        Task<IEnumerable<StoredConnectionSettings>> GetStored();
        Task<StoredConnectionSettings> Add(string displayName, bool storePassword, ConnectionSettings connectionSettings);
        Task<StoredConnectionSettings> Update(Guid id, string displayName, bool storePassword, ConnectionSettings connectionSettings);
        Task Delete(Guid id);
    }


    public class ConnectionSettings
    {
        public string Host { get; }
        public string VirtualHost { get; }
        public int Port { get; }
        public string Username { get; }
        public string Password { get; }

        public bool Subscribe { get; }
        public string Exchange { get; }
        public string RoutingKey { get; }


        public static readonly ConnectionSettings Default = new("localhost", "/", 5672, "guest", "guest", false, "", "#");


        public ConnectionSettings(string host, string virtualHost, int port, string username, string password, 
            bool subscribe, string exchange, string routingKey)
        {
            Host = host;
            VirtualHost = virtualHost;
            Port = port;
            Username = username;
            Password = password;

            Subscribe = subscribe;
            Exchange = exchange;
            RoutingKey = routingKey;
        }


        public bool SameParameters(ConnectionSettings value, bool comparePassword = true)
        {
            return Host == value.Host &&
                   VirtualHost == value.VirtualHost &&
                   Port == value.Port &&
                   Username == value.Username &&
                   (!comparePassword || Password == value.Password) &&
                   Subscribe == value.Subscribe &&
                   Exchange == value.Exchange &&
                   RoutingKey == value.RoutingKey;
        }
    }


    public class StoredConnectionSettings : ConnectionSettings
    {
        public Guid Id { get; }
        public string DisplayName { get; }
        public bool StorePassword { get; }


        public StoredConnectionSettings(Guid id, string displayName, bool storePassword, string host, string virtualHost, int port, string username,
            string password, bool subscribe, string exchange, string routingKey)
            : base(host, virtualHost, port, username, password, subscribe, exchange, routingKey)
        {
            Id = id;
            DisplayName = displayName;
            StorePassword = storePassword;
        }


        public StoredConnectionSettings(Guid id, string displayName, bool storePassword, ConnectionSettings connectionSettings)
            : base(connectionSettings.Host, connectionSettings.VirtualHost, connectionSettings.Port, connectionSettings.Username, 
                connectionSettings.Password, connectionSettings.Subscribe, connectionSettings.Exchange, connectionSettings.RoutingKey)
        {
            Id = id;
            DisplayName = displayName;
            StorePassword = storePassword;
        }
    }
}