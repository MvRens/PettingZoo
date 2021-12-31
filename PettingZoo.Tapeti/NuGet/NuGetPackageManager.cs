using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace PettingZoo.Tapeti.NuGet
{
    public class NuGetPackageManager : INuGetPackageManager
    {
        private readonly SourceCacheContext cache;
        private readonly List<Source> sources;

        public IReadOnlyList<INuGetPackageSource> Sources => sources;


        public NuGetPackageManager()
        {
            cache = new SourceCacheContext();
            sources = new List<Source>
            {
                new(cache, "nuget.org", @"https://api.nuget.org/v3/index.json")
            };
        }


        public NuGetPackageManager WithSourcesFrom(string nuGetConfig)
        {
            if (!File.Exists(nuGetConfig))
                return this;

            var doc = new XmlDocument();
            doc.Load(nuGetConfig);

            var nodes = doc.SelectNodes(@"/configuration/packageSources/add");
            if (nodes == null)
                return this;

            foreach (var entry in nodes.Cast<XmlNode>())
            {
                if (entry.Attributes == null)
                    continue;

                var nameAttribute = entry.Attributes["key"];
                var urlAttribute = entry.Attributes["value"];

                if (string.IsNullOrEmpty(nameAttribute?.Value) || string.IsNullOrEmpty(urlAttribute?.Value))
                    continue;

                sources.Add(new Source(cache, nameAttribute.Value, urlAttribute.Value));
            }

            return this;
        }



        private class Source : INuGetPackageSource
        {
            private readonly SourceCacheContext cache;
            private readonly SourceRepository repository;

            public string Name { get; }


            public Source(SourceCacheContext cache, string name, string url)
            {
                this.cache = cache;
                Name = name;
                repository = Repository.Factory.GetCoreV3(url);
            }


            public async Task<IReadOnlyList<INuGetPackage>> Search(string searchTerm, bool includePrerelease, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return Array.Empty<INuGetPackage>();

                var resource = await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
                var filter = new SearchFilter(includePrerelease);

                return (await resource.SearchAsync(searchTerm, filter, 0, 20, new NullLogger(),
                        cancellationToken))
                    .Select(p => new Package(cache, repository, p))
                    .ToArray();
            }
        }


        protected class Package : INuGetPackage
        {
            private readonly SourceCacheContext cache;
            private readonly SourceRepository repository;
            private readonly IPackageSearchMetadata packageSearchMetadata;

            public string Title => packageSearchMetadata.Title;
            public string Description => packageSearchMetadata.Description;
            public string Authors => packageSearchMetadata.Authors;
            public string Version => packageSearchMetadata.Identity.Version.ToString();


            private IReadOnlyList<INuGetPackageVersion>? versions;


            public Package(SourceCacheContext cache, SourceRepository repository, IPackageSearchMetadata packageSearchMetadata)
            {
                this.cache = cache;
                this.repository = repository;
                this.packageSearchMetadata = packageSearchMetadata;
            }


            public async Task<IReadOnlyList<INuGetPackageVersion>> GetVersions(CancellationToken cancellationToken)
            {
                return versions ??= (await packageSearchMetadata.GetVersionsAsync())
                    .Select(v => new PackageVersion(cache, repository, packageSearchMetadata, v.Version))
                    .ToArray();
            }
        }


        protected class PackageVersion : INuGetPackageVersion, IComparable<PackageVersion>
        {
            private readonly SourceCacheContext cache;
            private readonly SourceRepository repository;
            private readonly IPackageSearchMetadata packageSearchMetadata;

            protected readonly NuGetVersion NuGetVersion;


            public PackageVersion(SourceCacheContext cache, SourceRepository repository, IPackageSearchMetadata packageSearchMetadata, NuGetVersion nuGetVersion)
            {
                this.cache = cache;
                this.repository = repository;
                this.packageSearchMetadata = packageSearchMetadata;
                NuGetVersion = nuGetVersion;
            }


            public string Version => NuGetVersion.ToString();


            public async Task Download(Stream destination, CancellationToken cancellationToken)
            {
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
                await resource.CopyNupkgToStreamAsync(packageSearchMetadata.Identity.Id, NuGetVersion, destination, cache, new NullLogger(), cancellationToken);
            }


            public int CompareTo(INuGetPackageVersion? other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (other == null) return 1;

                return other is PackageVersion packageVersion ? CompareTo(packageVersion) : string.Compare(Version, other.Version, StringComparison.Ordinal);
            }

            public int CompareTo(PackageVersion? other)
            {
                if (ReferenceEquals(this, other)) return 0;
                return other == null ? 1 : NuGetVersion.CompareTo(other.NuGetVersion);
            }
        }
    }
}
