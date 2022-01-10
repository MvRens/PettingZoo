using System.Collections.Generic;

namespace PettingZoo.Core.Export
{
    public interface IExportFormatProvider
    {
        public IEnumerable<IExportFormat> Formats { get; }
    }
}
