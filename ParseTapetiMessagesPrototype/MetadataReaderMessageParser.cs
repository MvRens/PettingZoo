using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ParseTapetiMessagesPrototype
{
    public static class MetadataReaderMessageParser
    {
        public static void ParseAssembly(string classLibraryFilename)
        {
            try
            {
                using var fileStream = new FileStream(classLibraryFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var peReader = new PEReader(fileStream);

                var metadataReader = peReader.GetMetadataReader();

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var typeDefinitionHandle in metadataReader.TypeDefinitions)
                {
                    var typeDefinition = metadataReader.GetTypeDefinition(typeDefinitionHandle);
                    HandleTypeDefinition(metadataReader, typeDefinition);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private static void HandleTypeDefinition(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            var typeNamespace = metadataReader.GetString(typeDefinition.Namespace);
            var typeName = metadataReader.GetString(typeDefinition.Name);

            // For this prototype, filter out anything not ending in Message
            // Might want to show a full tree in PettingZoo since this is just a convention
            if (!typeName.EndsWith("Message"))
                return;

            Console.WriteLine($"{typeNamespace}.{typeName}");

            var example = new JObject();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var propertyDefinitionHandle in typeDefinition.GetProperties())
            {
                // TODO get properties from base class

                var propertyDefinition = metadataReader.GetPropertyDefinition(propertyDefinitionHandle);
                HandlePropertyDefinition(metadataReader, propertyDefinition, example);
            }

            Console.WriteLine(example.ToString());
            Console.WriteLine();
        }

        private static void HandlePropertyDefinition(MetadataReader metadataReader, PropertyDefinition propertyDefinition, JObject targetObject)
        {
            var fieldName = metadataReader.GetString(propertyDefinition.Name);
            var signature = propertyDefinition.DecodeSignature(new JsonSignatureProvider(), null);

            targetObject.Add(fieldName, signature.ReturnType);
        }


        private class JsonSignatureProvider : ISignatureTypeProvider<JToken, object>
        {
            public JToken GetPrimitiveType(PrimitiveTypeCode typeCode)
            {
                return typeCode switch
                {
                    PrimitiveTypeCode.Boolean => false,

                    PrimitiveTypeCode.Byte or
                        PrimitiveTypeCode.Int16 or
                        PrimitiveTypeCode.Int32 or
                        PrimitiveTypeCode.Int64 or
                        PrimitiveTypeCode.IntPtr or
                        PrimitiveTypeCode.SByte or
                        PrimitiveTypeCode.UInt16 or
                        PrimitiveTypeCode.UInt32 or
                        PrimitiveTypeCode.UInt64 or
                        PrimitiveTypeCode.UIntPtr => 0,

                    PrimitiveTypeCode.Char or
                        PrimitiveTypeCode.String => "",

                    PrimitiveTypeCode.Double or
                        PrimitiveTypeCode.Single => 0.0,

                    // TODO recurse
                    PrimitiveTypeCode.Object => "OBJECT",

                    _ => $"Unsupported primitive type code: {typeCode}"
                };
            }

            public JToken GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind = 0) => "typedef";

            public JToken GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind = 0)
            {
                var typeReference = reader.GetTypeReference(handle);
                var typeName = reader.GetString(typeReference.Name);

                return typeName;
            }
            

            public JToken GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind = 0) => "typespec";

            public JToken GetSZArrayType(JToken elementType) => new JValue(elementType + "[]");
            public JToken GetPointerType(JToken elementType) => null;
            public JToken GetByReferenceType(JToken elementType) => null;
            public JToken GetGenericMethodParameter(object genericContext, int index) => "!!" + index;
            public JToken GetGenericTypeParameter(object genericContext, int index) => "!" + index;

            public JToken GetPinnedType(JToken elementType) => elementType + " pinned";
            public JToken GetGenericInstantiation(JToken genericType, ImmutableArray<JToken> typeArguments) => genericType + "<" + string.Join(",", typeArguments) + ">";
            public JToken GetModifiedType(JToken modifierType, JToken unmodifiedType, bool isRequired) => unmodifiedType + (isRequired ? " modreq(" : " modopt(") + modifierType + ")";

            public JToken GetArrayType(JToken elementType, ArrayShape shape)
            {
                var builder = new StringBuilder();

                builder.Append(elementType);
                builder.Append('[');

                for (int i = 0; i < shape.Rank; i++)
                {
                    int lowerBound = 0;

                    if (i < shape.LowerBounds.Length)
                    {
                        lowerBound = shape.LowerBounds[i];
                        builder.Append(lowerBound);
                    }

                    builder.Append("...");

                    if (i < shape.Sizes.Length)
                    {
                        builder.Append(lowerBound + shape.Sizes[i] - 1);
                    }

                    if (i < shape.Rank - 1)
                    {
                        builder.Append(',');
                    }
                }

                builder.Append(']');
                return builder.ToString();
            }

            public JToken GetFunctionPointerType(MethodSignature<JToken> signature) => "methodptr(something)";
        }
    }
}
