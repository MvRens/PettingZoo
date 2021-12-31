using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace PettingZoo.Tapeti
{
    // TODO detect recursion
    // TODO detect recursion
    // TODO detect recursion
    // TODO stop making nerdy jokes in comments.

    // TODO generate at least one item for enumerables
    // TODO support basic types

    public class TypeToJObjectConverter
    {
        public static JObject Convert(Type type)
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
                    return new JArray(Convert(enumerableInterface.GetGenericArguments()[0]));


                return Convert(actualType);
            }

            if (actualType.IsArray)
                return new JArray(Convert(actualType.GetElementType()!));

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
