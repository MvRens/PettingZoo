using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PettingZoo.Core.Connection;

namespace PettingZoo.Core.Export
{
    public interface IExportFormat
    {
        public string Filter { get; }

        public Task Export(Stream stream, IEnumerable<ReceivedMessageInfo> messages);
    }
}
