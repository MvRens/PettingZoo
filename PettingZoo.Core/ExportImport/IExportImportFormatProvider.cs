using System.Collections.Generic;

namespace PettingZoo.Core.ExportImport
{
    public interface IExportImportFormatProvider
    {
        public IEnumerable<IExportFormat> ExportFormats { get; }
        public IEnumerable<IImportFormat> ImportFormats { get; }
    }
}
