using System.Collections.Generic;
using System.Linq;

namespace PettingZoo.Core.ExportImport
{
    public class ExportImportFormatProvider : IExportImportFormatProvider
    {
        private readonly List<IExportFormat> exportFormats;
        private readonly List<IImportFormat> importFormats;


        public IEnumerable<IExportFormat> ExportFormats => exportFormats;
        public IEnumerable<IImportFormat> ImportFormats => importFormats;


        public ExportImportFormatProvider(params IExportImportFormat[] formats)
        {
            exportFormats = new List<IExportFormat>(formats.Where(f => f is IExportFormat).Cast<IExportFormat>());
            importFormats = new List<IImportFormat>(formats.Where(f => f is IImportFormat).Cast<IImportFormat>());
        }
    }
}
