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
        public int Exp
        {
            get { return Stat.CurExp; }
            set { Stat.CurExp = Math.Clamp(value, 0, Stat.TotalExp); }
        }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }

        public Action<Player> _checkDeadTarget;

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);

            EquipWeapon = ObjectManager.Instance.CreateObjectWeapon(306);
            EquipWeapon.Owner = this;
            WeaponInfo.WeaponId = EquipWeapon.Id;

        }

        public override void Update()
        {
            // base.Update();
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }
        public override void OnDead(GameObject attacker)
        {

            _checkDeadTarget.Invoke(this);
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
                else if (item.ItemType == ItemType.Consumable)
                {

                }

                if (unEquipItem != null)
                {
                    //db
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

                //DB에 Noti
                DbTransaction.EquipItemNoti(this, item);

                //클라
                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equipOkItem);
            }
            RefreshCalcStat();
        }
        public void RefreshCalcStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

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
                        break;

                }
            }
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
