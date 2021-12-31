using LiteDB;
using LiteDB.Async;
using PettingZoo.Core.Settings;

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
            databaseFilename = Path.Combine(PettingZooPaths.AppDataRoot, $"{databaseName}.litedb");
        }


        protected ILiteDatabaseAsync GetDatabase()
        {
            return new LiteDatabaseAsync(databaseFilename, Mapper);
        }
    }
}
