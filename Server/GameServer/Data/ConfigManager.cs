using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameServer.Data
{
    [Serializable]
    public class ServerConfig
    {
        public string dataPath;
        public string connectionString;
        public ServerList serverList; 
    }

    public class ServerList
    {

        public string name;
        public int port; 
    }

    public class ConfigManager
    {
        public static ServerConfig Config { get; private set; }

        public static void LoadConfig()
        {
            string text = File.ReadAllText("../config.json");
            Config =  Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(text);
        }
    }
}
