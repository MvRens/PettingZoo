namespace PettingZoo.Settings
{
    public interface IUserSettingsSerializer
    {
        void Read(UserSettings settings);
        void Write(UserSettings settings);
    }


    public class ConnectionWindowSettings
    {
        public string LastHost { get; set; }
        public string LastVirtualHost { get; set; }
        public int LastPort { get; set; }
        public string LastUsername { get; set; }
        public string LastPassword { get; set; }

        //public bool LastSubscribe { get; set; }
        public string LastExchange { get; set; }
        public string LastRoutingKey { get; set;  }


        public ConnectionWindowSettings()
        {
            LastHost = "localhost";
            LastPort = 5672;
            LastVirtualHost = "/";
            LastUsername = "guest";
            LastPassword = "guest";

            LastExchange = "";
            LastRoutingKey = "#";            
        }
    }


    public class UserSettings
    {
        public ConnectionWindowSettings ConnectionWindow { get; }


        private readonly IUserSettingsSerializer serializer;
 

        public UserSettings(IUserSettingsSerializer serializer)
        {
            ConnectionWindow = new ConnectionWindowSettings();

            this.serializer = serializer;
            serializer.Read(this);
        }


        public void Save()
        {
            serializer.Write(this);
        }
    }
}
