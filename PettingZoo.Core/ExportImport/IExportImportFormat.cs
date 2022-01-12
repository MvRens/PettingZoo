using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PettingZoo.Core.Connection;

namespace PettingZoo.Core.ExportImport
{
    public interface IExportImportFormat
    {
        string Filter { get; }
    }


    public interface IExportFormat : IExportImportFormat
    {
        Task Export(Stream stream, IEnumerable<ReceivedMessageInfo> messages, CancellationToken cancellationToken);
    }


    public interface IImportFormat : IExportImportFormat
    {
        Task<IReadOnlyList<ReceivedMessageInfo>> Import(Stream stream, CancellationToken cancellationToken);
    }
}
