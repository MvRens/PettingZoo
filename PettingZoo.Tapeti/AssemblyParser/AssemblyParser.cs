using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Newtonsoft.Json;
using PettingZoo.Core.Generator;

namespace PettingZoo.Tapeti.AssemblyParser
{
    public class AssemblyParser : IDisposable
    {
        private readonly AssemblyLoadContext loadContext;

        public AssemblyParser(params string[] extraAssembliesPaths)
        {
            // Using the MetadataLoadContext introduces extra complexity since types can not be compared
            // (a string from the loaded assembly does not equal our typeof(string) for example).
            // So instead we'll use a regular AssemblyLoadContext. Not ideal, and will probably cause other side-effects
            // if we're not careful, but I don't feel like writing a full metadata parser right now.
            // If you have a better idea, it's open-source! :-)
            loadContext = new AssemblyLoadContext(null, true);

            foreach (var extraAssembly in extraAssembliesPaths.SelectMany(p => Directory.Exists(p)
                         ? Directory.GetFiles(p, "*.dll")
                         : Enumerable.Empty<string>()))
            {
                loadContext.LoadFromAssemblyPath(extraAssembly);
            }
        }


        public void Dispose()
        {
            loadContext.Unload();
            GC.SuppressFinalize(this);
        }


        public IEnumerable<IClassTypeExample> GetExamples(Stream assemblyStream)
        {
            var assembly = loadContext.LoadFromStream(assemblyStream);

            foreach (var type in assembly.GetTypes().Where(t => t.IsClass))
                yield return new TypeExample(type);
        }



        private class TypeExample : IClassTypeExample
        {
            public string AssemblyName => type.Assembly.GetName().Name ?? "";
            public string? Namespace => type.Namespace;
            public string ClassName => type.Name;

            private readonly Type type;


            public TypeExample(Type type)
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
    }
}
