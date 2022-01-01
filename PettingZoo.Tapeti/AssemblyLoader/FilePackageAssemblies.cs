using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PettingZoo.Tapeti.AssemblyLoader
{
    public class FilePackageAssemblies : IPackageAssemblies
    {
        private readonly string[] filenames;


        public FilePackageAssemblies(params string[] filenames)
        {
            this.filenames = filenames;
        }


        public Task<IEnumerable<IPackageAssembly>> GetAssemblies(IProgress<int> progress, CancellationToken cancellationToken)
        {
            return Task.FromResult(filenames.Select(f => (IPackageAssembly)new FilePackageAssembly(f)));
        }



        private class FilePackageAssembly : IPackageAssembly
        {
            private readonly string filename;


            public FilePackageAssembly(string filename)
            {
                this.filename = filename;
            }


            public Stream GetStream()
            {
                return new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
        }
    }
}
