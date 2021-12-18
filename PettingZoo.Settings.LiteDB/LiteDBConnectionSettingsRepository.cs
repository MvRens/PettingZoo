using PettingZoo.Core.Settings;

namespace PettingZoo.Settings.LiteDB
{
    public class LiteDBConnectionSettingsRepository : BaseLiteDBRepository, IConnectionSettingsRepository
    {
        private static readonly Guid LastUsedId = new("1624147f-76b2-4b5e-8e6f-2ef1730a0a99");

        private const string CollectionLastUsed = "lastUsed";
        private const string CollectionStored = "stored";


        public LiteDBConnectionSettingsRepository() : base(@"connectionSettings")
        {
        }


        public async Task<ConnectionSettings> GetLastUsed()
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<ConnectionSettingsRecord>(CollectionLastUsed);

            var lastUsed = await collection.FindOneAsync(r => true);
            if (lastUsed == null)
                return ConnectionSettings.Default;

            return new ConnectionSettings(
                lastUsed.Host,
                lastUsed.VirtualHost,
                lastUsed.Port,
                lastUsed.Username,
                lastUsed.Password,
                lastUsed.Subscribe,
                lastUsed.Exchange,
                lastUsed.RoutingKey);
        }


        public async Task StoreLastUsed(ConnectionSettings connectionSettings)
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<ConnectionSettingsRecord>(CollectionLastUsed);

            await collection.UpsertAsync(ConnectionSettingsRecord.FromConnectionSettings(LastUsedId, connectionSettings, ""));
        }


        public async Task<IEnumerable<StoredConnectionSettings>> GetStored()
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<ConnectionSettingsRecord>(CollectionStored);

            return (await collection.FindAllAsync())
                .Select(r => new StoredConnectionSettings(r.Id, r.DisplayName, r.Host, r.VirtualHost, r.Port, r.Username, r.Password, r.Subscribe, r.Exchange, r.RoutingKey))
                .ToArray();
        }


        public async Task<StoredConnectionSettings> Add(string displayName, ConnectionSettings connectionSettings)
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<ConnectionSettingsRecord>(CollectionStored);

            var id = Guid.NewGuid();
            await collection.InsertAsync(ConnectionSettingsRecord.FromConnectionSettings(id, connectionSettings, displayName));

            return new StoredConnectionSettings(id, displayName, connectionSettings);
        }


        public async Task<StoredConnectionSettings> Update(Guid id, string displayName, ConnectionSettings connectionSettings)
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<ConnectionSettingsRecord>(CollectionStored);

            await collection.UpdateAsync(ConnectionSettingsRecord.FromConnectionSettings(id, connectionSettings, displayName));
            return new StoredConnectionSettings(id, displayName, connectionSettings);
        }


        public async Task Delete(Guid id)
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<ConnectionSettingsRecord>(CollectionStored);

            await collection.DeleteAsync(id);
        }


        // ReSharper disable MemberCanBePrivate.Local - for LiteDB
        // ReSharper disable PropertyCanBeMadeInitOnly.Local
        private class ConnectionSettingsRecord
        {
            public Guid Id { get; set; }

            public string DisplayName { get; set; } = null!;
            public string Host { get; set; } = null!;
            public string VirtualHost { get; set; } = null!;
            public int Port { get; set; }
            public string Username { get; set; } = null!;
            public string? Password { get; set; }

            public bool Subscribe { get; set; }
            public string Exchange { get; set; } = null!;
            public string RoutingKey { get; set; } = null!;


            public static ConnectionSettingsRecord FromConnectionSettings(Guid id, ConnectionSettings connectionSettings, string displayName)
            {
                return new ConnectionSettingsRecord
                {
                    Id = id,
                    DisplayName = displayName,

                    Host = connectionSettings.Host,
                    VirtualHost = connectionSettings.VirtualHost,
                    Port = connectionSettings.Port,
                    Username = connectionSettings.Username,
                    Password = connectionSettings.Password,

                    Subscribe = connectionSettings.Subscribe,
                    Exchange = connectionSettings.Exchange,
                    RoutingKey = connectionSettings.RoutingKey
                };
            }
        }
        // ReSharper restore PropertyCanBeMadeInitOnly.Local
        // ReSharper restore MemberCanBePrivate.Local
    }
}