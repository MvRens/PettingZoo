using LiteDB;
using LiteDB.Async;

namespace PettingZoo.Settings.LiteDB
{
    public class BaseLiteDBRepository
    {
        private readonly string databaseFilename;

        protected static readonly BsonMapper Mapper = new()
        {
            EmptyStringToNull = false
        };


        public BaseLiteDBRepository(string databaseName)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (appDataPath == null)
                throw new IOException("Could not resolve application data path");

            var databasePath = Path.Combine(appDataPath, @"PettingZoo");
            if (!Directory.CreateDirectory(databasePath).Exists)
                throw new IOException($"Failed to create directory: {databasePath}");

            databaseFilename = Path.Combine(databasePath, $"{databaseName}.litedb");
        }


        protected ILiteDatabaseAsync GetDatabase()
        {
            return new LiteDatabaseAsync(databaseFilename, Mapper);
        }
    }
}
