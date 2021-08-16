using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
  
    #region Weapon
    [Serializable]
    public class Weapon
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