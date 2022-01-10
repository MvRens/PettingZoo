using System.Collections.Generic;

namespace PettingZoo.Core.Export
{
    public class ExportFormatProvider : IExportFormatProvider
    {
        private readonly List<IExportFormat> formats;

        public IEnumerable<IExportFormat> Formats => formats;

        public ExportFormatProvider(params IExportFormat[] formats)
        {
            this.formats = new List<IExportFormat>(formats);
        }
    }
}
