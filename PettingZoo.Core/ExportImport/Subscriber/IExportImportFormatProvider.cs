using System.Collections.Generic;

namespace PettingZoo.Core.ExportImport.Subscriber
{
    public interface IExportImportFormatProvider
    {
        public IEnumerable<IExportFormat> ExportFormats { get; }
        public IEnumerable<IImportFormat> ImportFormats { get; }
    }
}
