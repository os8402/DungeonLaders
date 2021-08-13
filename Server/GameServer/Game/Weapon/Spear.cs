using GameServer.Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Spear : EquipWeapon
    {
        public Spear(Weapon WeaponData)
        {
            this.WeaponData = WeaponData;
            Id = WeaponData.id;
            WeaponType = WeaponData.weaponType;

        }
        //창은 바라보는 방향 + attackRange만 계산해주면 됩니다.
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

    }
}
