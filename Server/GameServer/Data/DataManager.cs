using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameServer.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public static  Dictionary<int, StatInfo> StatDict { get; private set; } = new Dictionary<int, StatInfo>();
        public static  Dictionary<int, Data.WeaponSkillData> WeaponDict { get; private set; } = new Dictionary<int, Data.WeaponSkillData>();
        public static  Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, Data.ItemData>();
        public static  Dictionary<int, Data.MonsterData> MonsterDict { get; private set; } = new Dictionary<int, Data.MonsterData>();

        public static void LoadData()
        {
            StatDict = LoadJson<Data.StatData , int, StatInfo>("StatData").MakeDict();
            WeaponDict = LoadJson<Data.WeaponLoader, int, WeaponSkillData>("WeaponData").MakeDict();
            ItemDict = LoadJson<Data.ItemLoader , int, ItemData >("ItemData").MakeDict();
            MonsterDict = LoadJson<Data.MonsterLoader , int, MonsterData>("MonsterData").MakeDict();
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text); 

        }
    }

}
