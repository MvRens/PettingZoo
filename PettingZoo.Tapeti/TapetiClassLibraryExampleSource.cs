using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using PettingZoo.Core.Generator;

namespace PettingZoo.Tapeti
{
    public class TapetiClassLibraryExampleSource : IExampleSource
    {
        private readonly string classLibraryFilename;
        private readonly IEnumerable<string> extraAssemblies;
        private Lazy<AssemblySource> assemblySource;


        public TapetiClassLibraryExampleSource(string classLibraryFilename, IEnumerable<string> extraAssemblies)
        {
            this.classLibraryFilename = classLibraryFilename;
            this.extraAssemblies = extraAssemblies;

            assemblySource = new Lazy<AssemblySource>(AssemblySourceFactory);
        }


        public void Dispose()
        {
            if (assemblySource.IsValueCreated)
                assemblySource.Value.Dispose();

            GC.SuppressFinalize(this);
        }


        public IExampleFolder GetRootFolder()
        {
            return assemblySource.Value.RootFolder;
        }


        private AssemblySource AssemblySourceFactory()
        {
            var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

            var paths = runtimeAssemblies
                .Concat(extraAssemblies)
                .Append(classLibraryFilename);

            // TODO can we use a custom resolver to detect missing references?
            var resolver = new PathAssemblyResolver(paths);
            var loadContext = new MetadataLoadContext(resolver);
            try
            {
                var assembly = loadContext.LoadFromAssemblyPath(classLibraryFilename);
                var rootFolder = new Folder(@"Root");


                foreach (var assemblyType in assembly.GetTypes())
                    AddType(assemblyType, rootFolder);


                return new AssemblySource
                {
                    LoadContext = loadContext,
                    RootFolder = rootFolder
                };
            }
            catch
            {
                loadContext.Dispose();
                throw;
            }
        }


        private void AddType(Type type, Folder rootFolder)
        {
            if (!type.IsClass)
                return;

            var assemblyName = type.Assembly.GetName().Name + ".";
            var typeNamespace = type.Namespace ?? "";

            if (typeNamespace.StartsWith(assemblyName))
                typeNamespace = typeNamespace.Substring(assemblyName.Length);

            var folder = CreateFolder(rootFolder, typeNamespace);
            folder.AddMessage(new Message(type));
        }


        private static Folder CreateFolder(Folder rootFolder, string typeNamespace)
        {
            var parts = typeNamespace.Split('.');
            if (parts.Length == 0)
                return rootFolder;

            var folder = rootFolder;

            foreach (var part in parts)
                folder = folder.CreateFolder(part);

            return folder;
        }


        private class Folder : IExampleFolder
        {
            private readonly List<Folder> folders = new();
            private readonly List<IExampleMessage> messages = new();


            public string Name { get; }
            public IReadOnlyList<IExampleFolder> Folders => folders;
            public IReadOnlyList<IExampleMessage> Messages => messages;


            public Folder(string name)
            {
                Name = name;
            }


            public Folder CreateFolder(string name)
            {
                var folder = folders.FirstOrDefault(f => f.Name == name);
                if (folder != null)
                    return folder;

                folder = new Folder(name);
                folders.Add(folder);
                return folder;
            }


            public void AddMessage(IExampleMessage message)
            {
                messages.Add(message);
            }
        }


        private class Message : IExampleMessage
        {
            private readonly Type type;


            public Message(Type type)
            {
                this.type = type;
            }


            public string Generate()
            {
                /*
                  We can't create an instance of the type to serialize easily, as most will depend on
                  assemblies not included in the NuGet package, so we'll parse the Type ourselves.
                  This is still much easier than using MetadataReader, as we can more easily check against
                  standard types like Nullable.
                
                  The only external dependencies should be the attributes, like [RequiredGuid]. The messaging models
                  themselves should not inherit from classes outside of their assembly, or include properties
                  with types from other assemblies. With that assumption, walking the class structure should be safe.
                  The extraAssemblies passed to TapetiClassLibraryExampleSource can also be used to give it a better chance.
                */
                var serialized = TypeToJObjectConverter.Convert(type);
                return serialized.ToString(Formatting.Indented);
            }
        }


        private class AssemblySource : IDisposable
        {
            public MetadataLoadContext LoadContext { get; init; }
            public IExampleFolder RootFolder { get; init; }


            public void Dispose()
            {
                LoadContext.Dispose();
            }
        }
    }
}
