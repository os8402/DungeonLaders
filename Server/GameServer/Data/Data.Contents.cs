using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;


namespace GameServer.Data
{


    #region Stat

    [Serializable]
    public class StatData : ILoader<int, StatInfo>
    {
        public List<StatInfo> stats = new List<StatInfo>();

        public Dictionary<int, StatInfo> MakeDict()
        {
            Dictionary<int, StatInfo> dict = new Dictionary<int, StatInfo>();
            foreach (StatInfo stat in stats)
            {
                stat.Hp = stat.MaxHp;
                dict.Add(stat.Level, stat);
            }
               
            return dict;
        }
    }
    #endregion

    #region Weapon
    [Serializable]
    public class WeaponSkillData
    {
        public int id;
        public WeaponType weaponType;
        public string name;
        public float cooldown;
        public int damage;
        public SkillType skillType;
        public ProjectileInfo projectile;
    }

    public class ProjectileInfo
    {
        public string name;
        public float speed;
        public int range;
        public string prefab; 
    }

    public class WeaponLoader : ILoader<int, WeaponSkillData>
    {
        public List<WeaponSkillData> weapons = new List<WeaponSkillData>();

        public Dictionary<int, WeaponSkillData> MakeDict()
        {
            Dictionary<int, WeaponSkillData> dict = new Dictionary<int, WeaponSkillData>();
            foreach (WeaponSkillData weapon in weapons)
                dict.Add(weapon.id, weapon);
            return dict;
        }
    }

    #endregion

    #region Item
    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public ItemType itemType;

    }

    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
    }
    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defence;
    }
    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public int maxCount;
    }


    public class ItemLoader : ILoader<int, ItemData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public List<ArmorData> armors = new List<ArmorData>();
        public List<ConsumableData> consumables = new List<ConsumableData>();

        public Dictionary<int, ItemData> MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in weapons)
            {
                item.itemType = ItemType.Weapon;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in armors)
            {
                item.itemType = ItemType.Armor;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in consumables)
            {
                item.itemType = ItemType.Consumable;
                dict.Add(item.id, item);
            }

            return dict;
        }
    }

    #endregion

    #region Monster
    [Serializable]
    public class RewardData
    {

        public int probability;  //100분율
        public int itemId;
        public int count; 

    }

    [Serializable]
    public class MonsterData
    {
        public int id;
        public string name;
        public StatInfo stat;
        public List<RewardData> rewards; 
        //public string prefabPath; 
    }

    public class MonsterLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monser in monsters)
                dict.Add(monser.id, monser);
            return dict;
        }
    }

    #endregion
}
