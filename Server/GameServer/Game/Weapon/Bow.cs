using GameServer.Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Bow : EquipWeapon
    {
        public Bow(WeaponData weaponData)
        {
            this.Data = weaponData;
            Id = weaponData.id;
            WeaponType = weaponData.weaponType;

        }


        protected override List<AttackPos> CalcAttackRange(Vector2Int cellPos)
        {
            List<AttackPos> attackList = new List<AttackPos>();

            for (int i = 1; i <= AttackRange; i++)
            {
                AttackPos atkPos = new AttackPos();
                if (cellPos.x != 0 && cellPos.y != 0)
                {
                    atkPos.AttkPosX = (cellPos.x * i) ;
                    atkPos.AttkPosY = (cellPos.y * i) ;
                }
                else
                {
                    atkPos.AttkPosX = cellPos.x * i;
                    atkPos.AttkPosY = cellPos.y * i;

                }
                attackList.Add(atkPos);

            }

            return attackList;
        }

        protected override Vector2Int GetDirFromNormal(Vector2Int normal)
        {
            return new Vector2Int(normal.x, normal.y);
        }

        public void ShootArrow()
        {
            Arrow arrow = ObjectManager.Instance.Add<Arrow>();
            if (arrow == null)
                return;
            
            arrow.Owner = Owner;
            arrow.Data = Data;
            arrow.PosInfo.State = ControllerState.Moving;


            int posX = AttackList[0].AttkPosX;
            int posY = AttackList[0].AttkPosY;
 

            arrow.PosInfo.PosX = Owner.CellPos.x;
            arrow.PosInfo.PosY = Owner.CellPos.y;
      
            arrow.Dir = arrow.GetDirState(posX, posY);
            arrow.AttackPos = new AttackPos() { AttkPosX = posX, AttkPosY = posY };
            arrow.Info.Name = Data.projectile.name;
            arrow.TotalSpeed = Data.projectile.speed;

            arrow.StartCellPos = Owner.CellPos; 
            Owner.Room.Push(Owner.Room.EnterGame , arrow , false);
            Owner.Room.PushAfter(5 * 1000 ,  Owner.Room.LeaveGame, arrow.Id);



        }

        
    }
}
