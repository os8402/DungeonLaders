using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{


    #region Item
    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public string info;
        public ItemType itemType;
        public string iconPath;


    }
    public class ProjectileInfo
    {
        public string name;
        public float speed;
        public int range;
        public string prefab;
    }

    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
        public int attackRange;
        public float cooldown;
        public SkillType skillType;
        public ProjectileInfo projectile;
        public float effPosX;
        public float effPosY;
        public float handPosX;
        public float handPosY;
    }
    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defence;
        public int hp;
        public float speed;
    }
    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public int heal;
        public int maxCount;
    }


    [Serializable]
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
    public class MonsterData
    {
        public int id;
        public string name;
        public StatInfo stat; 
        public string prefabPath; 
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