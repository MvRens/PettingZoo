using PettingZoo.Core.Settings;

namespace PettingZoo.Settings.LiteDB
{
    public class LiteDBUISettingsRepository : BaseLiteDBRepository, IUISettingsRepository
    {
        private const string CollectionSettings = "settings";

        
        public LiteDBUISettingsRepository() : base(@"uiSettings")
        {
        }


        public async Task<MainWindowPositionSettings?> GetMainWindowPosition()
        {
            using var database = GetDatabase();
            var collection = database.GetCollection(CollectionSettings);

            var settings = await collection.FindByIdAsync(MainWindowPositionSettingsRecord.SettingsKey);
            if (settings == null)
                return null;

            var position = Mapper.ToObject<MainWindowPositionSettingsRecord>(settings);
            return new MainWindowPositionSettings(
                position.Top,
                position.Left,
                position.Width,
                position.Height,
                position.Maximized);
        }


        public async Task StoreMainWindowPosition(MainWindowPositionSettings settings)
        {
            using var database = GetDatabase();
            var collection = database.GetCollection(CollectionSettings);

            await collection.UpsertAsync(
                Mapper.ToDocument(new MainWindowPositionSettingsRecord
                {
                    Top = settings.Top,
                    Left = settings.Left,
                    Width = settings.Width,
                    Height = settings.Height,
                    Maximized = settings.Maximized
                }));
        }



        // ReSharper disable MemberCanBePrivate.Local - for LiteDB
        // ReSharper disable PropertyCanBeMadeInitOnly.Local
        private class BaseSettingsRecord
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public Guid Id { get; }

            protected BaseSettingsRecord(Guid id)
            {
                Id = id;
            }
        }


        private class MainWindowPositionSettingsRecord : BaseSettingsRecord
        {
            public static readonly Guid SettingsKey = new("fc71cf99-0744-4f5d-ada8-6a78d1df7b62");


            public int Top { get; set; }
            public int Left { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool Maximized { get; set; }

            public MainWindowPositionSettingsRecord() : base(SettingsKey)
            {
            }
        }
        // ReSharper restore PropertyCanBeMadeInitOnly.Local
        // ReSharper restore MemberCanBePrivate.Local
    }
}
