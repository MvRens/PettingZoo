using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PettingZoo.Tapeti
{
    public class TypeToJObjectConverter
    {
        public static JObject Convert(Type type)
        {
            if (!type.IsClass)
                throw new ArgumentException($"TypeToJObjectConverter.Convert expects a class, got {type.Name}");

            return ClassToJToken(type, Array.Empty<Type>());
        }


        private static readonly Dictionary<Type, Type> TypeEquivalenceMap = new()
        {
            { typeof(uint), typeof(int) },
            { typeof(long), typeof(int) },
            { typeof(ulong), typeof(int) },
            { typeof(short), typeof(int) },
            { typeof(ushort), typeof(int) },
            { typeof(float), typeof(decimal) }
        };


        private static readonly Dictionary<Type, JToken> TypeValueMap = new()
        {
            { typeof(int), 0 },
            { typeof(decimal), 0.0 },
            { typeof(bool), false }
        };


        private static JObject ClassToJToken(Type classType, IEnumerable<Type> typesEncountered)
        {
            var newTypesEncountered = typesEncountered.Append(classType).ToArray();
            var result = new JObject();

            foreach (var propertyInfo in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = TypeToJToken(propertyInfo.PropertyType, newTypesEncountered);
                result.Add(propertyInfo.Name, value);
            }

            return result;
        }


        private static JToken TypeToJToken(Type type, ICollection<Type> typesEncountered)
        {
            var actualType = Nullable.GetUnderlyingType(type) ?? type;

            if (TypeEquivalenceMap.TryGetValue(actualType, out var equivalentType))
                actualType = equivalentType;


            try
            {
                if (type.GetCustomAttribute<JsonConverterAttribute>() != null)
                {
                    // This type uses custom Json conversion so there's no way to know how to provide an example.
                    // We could try to create an instance of the type and pass it through the converter, but for now we'll
                    // just output a placeholder.
                    return "<custom JsonConverter - manual input required>";
                }
            }
            catch
            {
                // Move along
            }

            // String is also a class
            if (actualType == typeof(string))
                return "";


            if (actualType.IsClass)
            {
                // IEnumerable<T>
                var enumerableInterface = actualType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (enumerableInterface != null)
                    return new JArray(TypeToJToken(enumerableInterface.GetGenericArguments()[0], typesEncountered));

                return typesEncountered.Contains(actualType) ? new JValue((object?)null) : ClassToJToken(actualType, typesEncountered);
            }

            if (actualType.IsArray)
                return new JArray(TypeToJToken(actualType.GetElementType()!, typesEncountered));

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

            return TypeValueMap.TryGetValue(actualType, out var mappedToken)
                ? mappedToken
                : $"(unknown type: {actualType.Name})";
        }
    }
}
