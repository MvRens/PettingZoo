using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Newtonsoft.Json.Linq;

namespace ParseTapetiMessagesPrototype
{
    public static class AssemblyLoaderMessageParser
    {
        public static void ParseAssembly(string classLibraryFilename)
        {
            var loadContext = new AssemblyLoadContext(null, true);
            try
            {
                var assembly = loadContext.LoadFromAssemblyPath(classLibraryFilename);

                foreach (var assemblyType in assembly.GetTypes())
                    HandleType(assemblyType);
            }
            finally
            {
                loadContext.Unload();
            }
        }

        
        private static void HandleType(Type type)
        {
            if (!type.IsClass)
                return;
            
            // For this prototype, filter out anything not ending in Message
            // Might want to show a full tree in PettingZoo since this is just a convention
            if (!type.Name.EndsWith("Message") || type.Name != "RelatieUpdateMessage")
                return;

            Console.WriteLine($"{type.Namespace}.{type.Name}");

            // We can't create an instance of the type to serialize easily, as most will depend on
            // assemblies not included in the NuGet package, so we'll parse the Type ourselves.
            // This is still slightly easier than using MetadataReader, as we can more easily check against
            // standard types like Nullable.
            //
            // The only external dependencies should be the attributes, like [RequiredGuid]. The messaging models
            // themselves should not inherit from classes outside of their assembly, or include properties
            // with types from other assemblies. With that assumption, walking the class structure should be safe.
            var serialized = TypeToJObject(type);

            Console.WriteLine(serialized);
            Console.WriteLine("");
        }
        
        
        private static JObject TypeToJObject(Type type)
        {
            var result = new JObject();
            
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Note: unfortunately we can not call GetCustomAttributes here, as that would
                // trigger assemblies not included in the package to be loaded
                
                var value = PropertyToJToken(propertyInfo.PropertyType);
                result.Add(propertyInfo.Name, value);
            }

            return result;
        }


        private static readonly Dictionary<Type, JToken> TypeMap = new()
        {
            { typeof(short), 0 },
            { typeof(ushort), 0 },
            { typeof(int), 0 },
            { typeof(uint), 0 },
            { typeof(long), 0 },
            { typeof(ulong), 0 },
            { typeof(decimal), 0.0 },
            { typeof(float), 0.0 },
            { typeof(bool), false }
        };
        
        
        private static JToken PropertyToJToken(Type propertyType)
        {
            var actualType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            
            // String is also a class
            if (actualType == typeof(string))
                return "";
            
            
            if (actualType.IsClass)
            {
                // IEnumerable<T>
                var enumerableInterface = actualType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                
                if (enumerableInterface != null)
                    return new JArray(TypeToJObject(enumerableInterface.GetGenericArguments()[0]));
                
                
                return TypeToJObject(actualType);
            }

            if (actualType.IsArray)
                return new JArray(TypeToJObject(actualType.GetElementType()));
            
            if (actualType.IsEnum)
                return Enum.GetNames(actualType).FirstOrDefault();

            
            // Special cases for runtime generated values
            if (actualType == typeof(DateTime))
            {
                // Strip the milliseconds for a cleaner result
                var now = DateTime.UtcNow;
                return new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, now.Kind);
            }

            if (actualType == typeof(Guid))
                return Guid.NewGuid().ToString();

            return TypeMap.TryGetValue(actualType, out var mappedToken)
                ? mappedToken
                : $"(unknown type: {actualType.Name})";
        }
    }
}
