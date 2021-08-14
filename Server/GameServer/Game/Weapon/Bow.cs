using GameServer.Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Bow : EquipWeapon
    {
        public Bow(Weapon WeaponData)
        {
            this.Data = WeaponData;
            Id = WeaponData.id;
            WeaponType = WeaponData.weaponType;

        }


        //활은 바라보고있던 1방향만 처리해주면 됩니다 .
        // 그 후 투사체를 생성해서 투사체에 닿으면
        // 데미지 판정을 받는 식으로 설계할 예정
        //  ㅁ  ㅁ  ㅁ        
        //  ㅁ  me  ㅁ
        //  ㅁ  ㅁ  ㅁ    
        protected override List<AttackPos> CalcAttackRange(Vector2Int cellPos, int range)
        {
            List<AttackPos> attackList = new List<AttackPos>();

            for (int i = 1; i <= range; i++)
                attackList.Add(
                    new AttackPos
                    {
                        AttkPosX = cellPos.x * i,
                        AttkPosY = cellPos.y * i
                    });

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
            arrow.WeaponData = Data;
            arrow.PosInfo.State = ControllerState.Moving;

            int posX = AttackList[0].AttkPosX;
            int posY = AttackList[0].AttkPosY;
            
            arrow.PosInfo.PosX = Owner.CellPos.x;
            arrow.PosInfo.PosY = Owner.CellPos.y;
            arrow.Dir = arrow.GetDirState(posX, posY);
            arrow.AttackPos = new AttackPos() { AttkPosX = posX, AttkPosY = posY };
            arrow.Speed = Data.projectile.speed;
            Owner.Room.Push(Owner.Room.EnterGame , arrow);
        }

        
    }
}
