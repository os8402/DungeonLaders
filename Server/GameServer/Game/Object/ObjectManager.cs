﻿using GameServer.Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();
        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        //[UNUSED[1]] {TYPE(7)} {ID(24)}
        //[......... |
        int _counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock (_lock)
            {
                gameObject.Id = GenerateId(gameObject.ObjectType);

                if (gameObject.ObjectType == GameObjectType.Player)
                {
                    _players.Add(gameObject.Id, gameObject as Player);
                }
            }

            return gameObject;
        }

        int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24) | (_counter++);
            }
        }

        public static GameObjectType GetObjectTypeId(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public EquipWeapon CreateObjectWeapon()
        {
            List<int> weaponList = new List<int>();

            foreach (ItemData item in DataManager.ItemDict.Values)
            {
                if (item.itemType == ItemType.Weapon)
                    weaponList.Add(item.id);
            }

            Random rand = new Random();
            int id = rand.Next(0, weaponList.Count);

            return CreateObjectWeapon(weaponList[id]);
        }
        //무기 생성
        public EquipWeapon CreateObjectWeapon(int id)
        {

            ItemData itemData = null;

            int key = id;


            if (DataManager.ItemDict.ContainsKey(key) == false)
                return null;

            if (DataManager.ItemDict.TryGetValue(key, out itemData) == false)
                return null;

            EquipWeapon equipWeapon = null;
            WeaponData weaponData = (WeaponData)itemData;

            switch (weaponData.weaponType)
            {
                case WeaponType.Sword:
                    equipWeapon = new Sword(weaponData);
                    break;
                case WeaponType.Spear:
                    equipWeapon = new Spear(weaponData);
                    break;
                case WeaponType.Bow:
                    equipWeapon = new Bow(weaponData);
                    break;
                case WeaponType.Staff:
                    equipWeapon = new Staff(weaponData);
                    break;
            }

            return equipWeapon;
        }

        public bool Remove(int objectId)
        {
            GameObjectType objectType = GetObjectTypeId(objectId);


            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                    return _players.Remove(objectId);
            }

            return false;

        }
        public Player Find(int objectId)
        {
            GameObjectType objectType = GetObjectTypeId(objectId);

            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                {
                    Player player = null;
                    if (_players.TryGetValue(objectId, out player))
                        return player;
                }

                return null;
            }
        }
    }
}
