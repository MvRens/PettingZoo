using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PettingZoo.Tapeti.AssemblyLoader
{
    public interface IPackageAssemblies
    {
        Task<IEnumerable<IPackageAssembly>> GetAssemblies(IProgress<int> progress, CancellationToken cancellationToken);
    }


    public interface IPackageAssembly
    {
        Stream GetStream();
    }
}
