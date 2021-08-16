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
        public Inventory Inven { get; private set; } = new Inventory();

        public Action<Player> _checkDeadTarget;

        public Player()
        {
            ObjectType = GameObjectType.Player;

            EquipWeapon = ObjectManager.Instance.CreateObjectWeapon(-1);
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

            DbTransaction.SavePlayerStatus_AllInOne(this, Room);
        }

    }
}
