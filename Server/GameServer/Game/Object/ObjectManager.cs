using GameServer.Data;
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

        public EquipWeapon CreateObjectWeapon(int id = -1)
        {

            WeaponSkillData weapon = null;
            List<int> keyList = DataManager.WeaponDict.Keys.ToList();

            if (id == -1)
            {
                Random rand = new Random();
                id = rand.Next(0, keyList.Count);
            }


            if (DataManager.WeaponDict.ContainsKey(keyList[id]) == false)
                return null;

            if (DataManager.WeaponDict.TryGetValue(keyList[id], out weapon) == false)
                return null;

            EquipWeapon equipWeapon = null;

            switch (weapon.weaponType)
            {
                case WeaponType.Sword:
                    equipWeapon = new Sword(weapon);
                    break;
                case WeaponType.Spear:
                    equipWeapon = new Spear(weapon);
                    break;
                case WeaponType.Bow:
                    equipWeapon = new Bow(weapon);
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
