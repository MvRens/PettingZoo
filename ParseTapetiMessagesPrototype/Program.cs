namespace ParseTapetiMessagesPrototype
{
    public class Program
    {
        public static void Main()
        {
            const string classLibraryFilename = "D:\\Temp\\lib\\netstandard2.0\\Messaging.Relatie.dll";

            // There are advantages to using the MetadataReader, for example no code is run (LoadAssemblyForReflection is no longer
            // supported in .NET Core) and the assembly is not locked at all. This comes at the cost of complexity however, so
            // this prototype explores both options.
            //
            // In the final version perhaps we can work around loading the assembly into our own process by spawning a new process
            // to convert it into metadata used by the main process.
            
            //MetadataReaderMessageParser.ParseAssembly(classLibraryFilename);
            
            AssemblyLoaderMessageParser.ParseAssembly(classLibraryFilename);
        }
    }
}
