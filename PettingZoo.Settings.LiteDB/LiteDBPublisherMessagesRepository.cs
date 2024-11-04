using PettingZoo.Core.ExportImport.Publisher;
using PettingZoo.Core.Settings;

namespace PettingZoo.Settings.LiteDB
{
    public class LiteDBPublisherMessagesRepository : BaseLiteDBRepository, IPublisherMessagesRepository
    {
        private const string CollectionMessages = "messages";


        public LiteDBPublisherMessagesRepository() : base(@"publisherMessages")
        {
        }


        public async Task<IEnumerable<StoredPublisherMessage>> GetStored()
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<PublisherMessageRecord>(CollectionMessages);

            return (await collection.FindAllAsync())
                .Select(r => new StoredPublisherMessage(r.Id, r.DisplayName, r.Message))
                .ToArray();
        }


        public async Task<StoredPublisherMessage> Add(string displayName, PublisherMessage message)
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<PublisherMessageRecord>(CollectionMessages);

            var id = Guid.NewGuid();
            await collection.InsertAsync(PublisherMessageRecord.FromPublisherMessage(id, displayName, message));

            return new StoredPublisherMessage(id, displayName, message);
        }


        public async Task<StoredPublisherMessage> Update(Guid id, string displayName, PublisherMessage message)
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<PublisherMessageRecord>(CollectionMessages);

            await collection.UpdateAsync(PublisherMessageRecord.FromPublisherMessage(id, displayName, message));
            return new StoredPublisherMessage(id, displayName, message);
        }


        public async Task Delete(Guid id)
        {
            using var database = GetDatabase();
            var collection = database.GetCollection<PublisherMessageRecord>(CollectionMessages);

            await collection.DeleteAsync(id);
        }


        // ReSharper disable MemberCanBePrivate.Local - for LiteDB
        // ReSharper disable PropertyCanBeMadeInitOnly.Local
        private class PublisherMessageRecord
        {
            public Guid Id { get; set; }
            public string DisplayName { get; set; } = null!;
            public PublisherMessage Message { get; set; } = null!;


            public static PublisherMessageRecord FromPublisherMessage(Guid id, string displayName, PublisherMessage message)
            {
                return new PublisherMessageRecord
                {
                    Id = id,
                    DisplayName = displayName,
                    Message = message
                };
            }
        }
        // ReSharper restore PropertyCanBeMadeInitOnly.Local
        // ReSharper restore MemberCanBePrivate.Local
    }
}