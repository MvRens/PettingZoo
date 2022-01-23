using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PettingZoo.Benchmark
{
    /*
    

        Small JSON


        |           Method |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
        |----------------- |----------:|----------:|----------:|-------:|----------:|
        |  TestJsonConvert | 13.226 us | 0.1808 us | 0.1603 us | 3.6316 |     15 KB |
        |  TestJTokenParse | 12.360 us | 0.2453 us | 0.5010 us | 3.6011 |     15 KB |
        | TestReaderWriter |  6.398 us | 0.1260 us | 0.1294 us | 2.0294 |      8 KB |
        | TestJsonDocument |  4.400 us | 0.0758 us | 0.0902 us | 2.1019 |      9 KB |



        Large JSON (25 MB)

        |           Method |       Mean |    Error |   StdDev |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
        |----------------- |-----------:|---------:|---------:|-----------:|-----------:|----------:|----------:|
        |  TestJsonConvert | 1,331.6 ms | 20.36 ms | 19.05 ms | 57000.0000 | 21000.0000 | 3000.0000 |    390 MB |
        |  TestJTokenParse | 1,411.0 ms | 27.28 ms | 24.18 ms | 62000.0000 | 23000.0000 | 4000.0000 |    415 MB |
        | TestReaderWriter |   298.6 ms |  5.89 ms |  9.34 ms | 25000.0000 |  8000.0000 | 2000.0000 |    199 MB |
        | TestJsonDocument |   278.5 ms |  5.29 ms |  6.30 ms |          - |          - |         - |    246 MB |
            

    */
    [MemoryDiagnoser]
    public class JsonPrettyPrint
    {
        // Small Json file, which is likely to be typical for most RabbitMQ messages.
        private const string Json = "{\"glossary\":{\"title\":\"example glossary\",\"GlossDiv\":{\"title\":\"S\",\"GlossList\":{\"GlossEntry\":{\"ID\":\"SGML\",\"SortAs\":\"SGML\",\"GlossTerm\":\"Standard Generalized Markup Language\",\"Acronym\":\"SGML\",\"Abbrev\":\"ISO 8879:1986\",\"GlossDef\":{\"para\":\"A meta-markup language, used to create markup languages such as DocBook.\",\"GlossSeeAlso\":[\"GML\",\"XML\"]},\"GlossSee\":\"markup\"}}}}}";

        // To test with a large file instead, specify the file name here.
        // For example, I've benchmarked it with this 25 MB JSON file: https://raw.githubusercontent.com/json-iterator/test-data/master/large-file.json
        //private const string JsonFilename = "";
        private const string JsonFilename = "D:\\Temp\\large-file.json";


        private readonly string testJson;


        public JsonPrettyPrint()
        {
            testJson = string.IsNullOrEmpty(JsonFilename)
                ? Json
                : File.ReadAllText(JsonFilename);
        }


        [Benchmark]
        public string TestJsonConvert()
        {
            var obj = JsonConvert.DeserializeObject(testJson);
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }


        [Benchmark]
        public string TestJTokenParse()
        {
            var obj = JToken.Parse(testJson);
            return obj.ToString(Formatting.Indented);
        }


        [Benchmark]
        public string TestReaderWriter()
        {
            using var stringReader = new StringReader(testJson);
            using var jsonTextReader = new JsonTextReader(stringReader);
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);

            jsonWriter.Formatting = Formatting.Indented;

            while (jsonTextReader.Read())
                jsonWriter.WriteToken(jsonTextReader);

            jsonWriter.Flush();
            return stringWriter.ToString();
        }


        [Benchmark]
        public string TestJsonDocument()
        {
            var doc = JsonDocument.Parse(testJson);

            using var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            doc.WriteTo(writer);
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }


        
    }


    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<JsonPrettyPrint>();

            // To prove they all provide the correct output
            /*
            var prettyPrint = new JsonPrettyPrint();
            Console.WriteLine("JsonConvert");
            Console.WriteLine("-----------");
            Console.WriteLine(prettyPrint.TestJsonConvert());

            Console.WriteLine("JToken");
            Console.WriteLine("------");
            Console.WriteLine(prettyPrint.TestJTokenParse());

            Console.WriteLine("ReaderWriter");
            Console.WriteLine("------------");
            Console.WriteLine(prettyPrint.TestReaderWriter());

            Console.WriteLine("JsonDocument");
            Console.WriteLine("------------");
            Console.WriteLine(prettyPrint.TestJsonDocument());
            */
        }
    }
}