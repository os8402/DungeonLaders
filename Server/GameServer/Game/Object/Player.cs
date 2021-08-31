using GameServer.Data;
using GameServer.DB;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public VisionCube Vision { get; set; }
        public Inventory Inven { get; private set; } = new Inventory();

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }
        public int ArmorHp { get; private set; }
        public float ArmorSpeed { get; private set; }

        public int Exp
        {
            get { return Stat.CurExp; }
            set { Stat.CurExp = Math.Clamp(value, 0, Stat.TotalExp); }
        }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }
        public override int TotalHp { get { return Stat.MaxHp + ArmorHp; } }
   
        public override float TotalSpeed { get { return Stat.Speed + ArmorSpeed; } }
     

        public Action<Player> _checkDeadTarget;

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }

 

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }
        public override void OnDead(GameObject attacker)
        {

            _checkDeadTarget?.Invoke(this);
            base.OnDead(attacker);
        }

        public void OnLeaveGame()
        {
            //TODO
            //1)피가 깎일 때마다 DB접근 할 필요가 있나 
            //2)서버 다운되면 저장되지 않은 정보 날라감
            //3)코드 흐름을 다 막아버림

            DbTransaction.SavePlayerStatus_Hp(this, Room);
            DbTransaction.SavePlayerStatus_Exp(this, Room);
        }


        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
            if (item == null)
                return;

            if (item.ItemType == ItemType.Consumable)
                return;

            //착용 요청 -> 겹치는 부위 해제
            if (equipPacket.Equipped)
            {
                Item unEquipItem = null;
                if (item.ItemType == ItemType.Weapon)
                {
                    unEquipItem =
                        Inven.Find(i => i.Equipped && i.ItemType == ItemType.Weapon);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unEquipItem =
                        Inven.Find(i => i.Equipped && i.ItemType == ItemType.Armor
                        && ((Armor)i).ArmorType == armorType);

                }
   
                if (unEquipItem != null)
                {
                   
                    //메모리 선 적용 [중요하지않은 데이터들 ]
                    unEquipItem.Equipped = false;
    

                    //DB에 Noti
                    DbTransaction.EquipItemNoti(this, unEquipItem);

                    //클라
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unEquipItem.ItemDbId;
                    equipOkItem.Equipped = unEquipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }

            {
                //db
                //메모리 선 적용 [중요하지않은 데이터들 ]
                item.Equipped = equipPacket.Equipped;

                if (item.ItemType == ItemType.Weapon)
                {
                    if(item.Equipped == false)
                    {
                        EquipWeapon = null;
                    }
                    else
                    {
                        EquipWeapon = ObjectManager.Instance.CreateObjectWeapon(item.TemplateId);
                    
                    }

                    //무기는 변경사실을 모두에게 전달
                    S_ChangeWeapon changePacket = new S_ChangeWeapon();
                    changePacket.ObjectId = Id;
                    changePacket.TemplateId = item.TemplateId;
                    changePacket.Equipped = item.Equipped;

                    Room.Broadcast(CellPos, changePacket);

                }

                //DB에 Noti
                DbTransaction.EquipItemNoti(this, item);

                //클라
                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equipOkItem);

          
            }

            RefreshCalcStat();

            S_ChangeHp hpPacket = new S_ChangeHp();
            hpPacket.ObjectId = Id;
            hpPacket.Hp = HP;
            hpPacket.TotalHp = TotalHp;
            Room.Broadcast(CellPos, hpPacket);
        }

        public void HandleUseItem(C_UseItem usePacket)
        {
            Item item = Inven.Get(usePacket.ItemDbId);
            if (item == null)
                return;

            if (item.ItemType != ItemType.Consumable)
                return;


            DbTransaction.UseItem(this, item, usePacket.UseCount  , Room);

        }
        public void HandleRemoveItem(C_RemoveItem removePacket)
        {
            Item item = Inven.Get(removePacket.ItemDbId);
            if (item == null)
                return;

            DbTransaction.RemoveItem(this, item, Room); 
        }

        public void RefreshCalcStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;
            ArmorHp = 0;
            ArmorSpeed = 0; 

            foreach (Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        ArmorHp += ((Armor)item).Hp;
                        ArmorSpeed += ((Armor)item).Speed;
                        break;

                }
            }

            Info.TotalHp = TotalHp;
        }

        public void HandleLevelUp(C_LevelUp upPacket)
        {
            Exp = 0;
            StatInfo stat = null;
            DataManager.StatDict.TryGetValue(Stat.Level + 1, out stat);

            Stat.MergeFrom(stat);
            DbTransaction.SavePlayerStatus_All(this, Room);

        }



    }

}
