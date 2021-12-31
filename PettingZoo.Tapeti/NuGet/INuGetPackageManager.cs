using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PettingZoo.Tapeti.NuGet
{
    // TODO support logger
    public interface INuGetPackageManager
    {
        public IReadOnlyList<INuGetPackageSource> Sources { get; }
    }


    public interface INuGetPackageSource
    {
        public string Name { get; }

        public Task<IReadOnlyList<INuGetPackage>> Search(string searchTerm, bool includePrerelease, CancellationToken cancellationToken);
    }


    public interface INuGetPackage
    {
        public string Title { get; }
        public string Description { get; }
        public string Authors { get; }
        public string Version { get; }

        public Task<IReadOnlyList<INuGetPackageVersion>> GetVersions(CancellationToken cancellationToken);
    }
    

    public interface INuGetPackageVersion : IComparable<INuGetPackageVersion>
    {
        public string Version { get; }

        // TODO support fetching dependencies
        public Task Download(Stream destination, CancellationToken cancellationToken);
    }
}
