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
    public class Weapon
    {
        public int id;
        public Weapons weaponType;
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

    public class WeaponData : ILoader<int, Weapon>
    {
        public List<Weapon> weapons = new List<Weapon>();

        public Dictionary<int, Weapon> MakeDict()
        {
            Dictionary<int, Weapon> dict = new Dictionary<int, Weapon>();
            foreach (Weapon weapon in weapons)
                dict.Add(weapon.id, weapon);
            return dict;
        }
    }

    #endregion
}
