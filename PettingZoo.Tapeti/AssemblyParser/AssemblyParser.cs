using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using PettingZoo.Core.Generator;
using Tapeti.DataAnnotations.Extensions;

namespace PettingZoo.Tapeti.AssemblyParser
{
    public class AssemblyParser : IDisposable
    {
        private readonly MetadataLoadContext loadContext;

        public AssemblyParser(params string[] extraAssemblies)
        {
            var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            var paths = runtimeAssemblies
                .Concat(extraAssemblies)

                // TODO find a cleaner way
                .Append(typeof(JsonSerializer).Assembly.Location)
                .Append(typeof(RequiredGuidAttribute).Assembly.Location);


            var resolver = new PathAssemblyResolver(paths);
            loadContext = new MetadataLoadContext(resolver);
        }


        public void Dispose()
        {
            loadContext.Dispose();
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
