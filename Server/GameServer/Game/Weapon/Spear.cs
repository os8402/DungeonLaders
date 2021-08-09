using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Spear : Weapon
    {
        //창은 바라보는 방향 + attackRange만 계산해주면 됩니다.
        //     ㅁ           
        // ㅁ  me  ㅁ
        //     ㅁ      
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

        protected override Vector2Int GetDirFromNormal(Vector2Int pos)
        {

            int x = CalcDirFromSpear(pos.x);
            int y = CalcDirFromSpear(pos.y);
            return new Vector2Int(x, y);

        }

        int CalcDirFromSpear(int num)
        {
            int temp = 0;
            if (num >= 0.05f) temp = 1;
            else if (num <= -0.05f) temp = -1;

            return temp;
        }
    }
}
