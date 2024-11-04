using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PettingZoo.Core.ExportImport.Publisher;

namespace PettingZoo.Core.Settings
{
    public interface IPublisherMessagesRepository
    {
        // For now read everything into memory, you need quite a few and/or huge messsages before that becomes an issue
        Task<IEnumerable<StoredPublisherMessage>> GetStored();

        Task<StoredPublisherMessage> Add(string displayName, PublisherMessage message);
        Task<StoredPublisherMessage> Update(Guid id, string displayName, PublisherMessage message);
        Task Delete(Guid id);
    }


    public class StoredPublisherMessage
    {
        public Guid Id { get; }
        public string DisplayName { get; }
        public PublisherMessage Message { get; }


        public StoredPublisherMessage(Guid id, string displayName, PublisherMessage message)
        {
            Id = id;
            DisplayName = displayName;
            Message = message;
        }
    }
}