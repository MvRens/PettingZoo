using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging;
using PettingZoo.Tapeti.NuGet;

namespace PettingZoo.Tapeti.AssemblyLoader
{
    public class NuGetPackageAssemblies : IPackageAssemblies
    {
        private readonly INuGetPackageVersion packageVersion;


        public NuGetPackageAssemblies(INuGetPackageVersion packageVersion)
        {
            this.packageVersion = packageVersion;
        }


        public async Task<IEnumerable<IPackageAssembly>> GetAssemblies(IProgress<int> progress, CancellationToken cancellationToken)
        {
            await using var packageStream = new MemoryStream();
            await packageVersion.Download(packageStream, cancellationToken);

            packageStream.Seek(0, SeekOrigin.Begin);
            using var packageReader = new PackageArchiveReader(packageStream);

            // Determine which frameworks versions PettingZoo is compatible with so that we can actually load the assemblies
            var targetFrameworkAttribute = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                .Cast<TargetFrameworkAttribute>()
                .Single();

            var targetFramework = NuGetFramework.ParseFrameworkName(targetFrameworkAttribute.FrameworkName, DefaultFrameworkNameProvider.Instance);

            var libVersion = (await packageReader.GetLibItemsAsync(cancellationToken))
                .Where(l => DefaultCompatibilityProvider.Instance.IsCompatible(targetFramework, l.TargetFramework))
                .OrderByDescending(l => l.TargetFramework)
                .FirstOrDefault();

            if (libVersion == null)
                return Enumerable.Empty<IPackageAssembly>();


            var assemblies = new List<IPackageAssembly>();

            foreach (var filename in libVersion.Items.Where(f => f.EndsWith(@".dll", StringComparison.InvariantCultureIgnoreCase)))
            {
                var assembly = await new NuGetPackageAssembly().CopyFrom(packageReader.GetStream(filename));
                assemblies.Add(assembly);
            }

            return assemblies;
        }



        private class NuGetPackageAssembly : IPackageAssembly
        {
            private readonly MemoryStream buffer = new();


            public async Task<IPackageAssembly> CopyFrom(Stream stream)
            {
                await stream.CopyToAsync(buffer);
                return this;
            }


            public Stream GetStream()
            {
                return new MemoryStream(buffer.GetBuffer());
            }
        }
    }
}
