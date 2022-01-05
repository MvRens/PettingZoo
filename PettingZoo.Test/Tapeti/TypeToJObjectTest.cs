using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using PettingZoo.Tapeti;
using Xunit;

#nullable disable

namespace PettingZoo.Test.Tapeti
{
    public class TypeToJObjectTest
    {
        [Fact]
        public void TestAllSupportedTypes()
        {
            var converted = TypeToJObjectConverter.Convert(typeof(AllSupportedTypesTest));

            using (new AssertionScope())
            {
                // Directly supported
                converted.Should().HaveElement("StringValue").Which.Should().HaveValue("");
                converted.Should().HaveElement("IntValue").Which.Should().Match(t => t.Type == JTokenType.Integer);
                converted.Should().HaveElement("BoolValue").Which.Should().Match(t => t.Type == JTokenType.Boolean);

                var guidValue = converted.Should().HaveElement("GuidValue").Which.Should().Match(t => t.Type == JTokenType.String)
                    .And.Subject.Value<string>();
                Guid.TryParse(guidValue, out _).Should().BeTrue();


                var objectValue = converted.Should().HaveElement("ObjectValue").Subject;
                objectValue.Should().HaveElement("SubStringValue").Which.Should().HaveValue("");
                objectValue.Should().HaveElement("SubIntValue").Which.Should().Match(t => t.Type == JTokenType.Integer);
                objectValue.Should().HaveElement("RecursiveValue").Which.Type.Should().Be(JTokenType.Null);

                // Via type mapping
                // TODO test type mappings
            }
        }


        // ReSharper disable UnusedMember.Local
        private class AllSupportedTypesTest
        {
            public string StringValue { get; set; }
            public int IntValue { get; set; }
            public bool BoolValue { get; set; }

            public Guid GuidValue { get; set; }

            public ClassProperty ObjectValue { get; set; }
        }


        // ReSharper disable once ClassNeverInstantiated.Local
        private class ClassProperty
        {
            public string SubStringValue { get; set; }
            public int SubIntValue { get; set; }

            public AllSupportedTypesTest RecursiveValue { get; set; }
        }
        // ReSharper restore UnusedMember.Local
    }
}