using GameServer.Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Staff : EquipWeapon
    {
        public Staff(WeaponData weaponData)
        {
            this.Data = weaponData;
            Id = weaponData.id;
            WeaponType = weaponData.weaponType;

        }

        int[] dx = { 0, 0, 1, -1 };
        int[] dY = { 1, -1, 0, 0 };

        protected override List<AttackPos> CalcAttackRange(Vector2Int cellPos)
        {
            List<AttackPos> attackList = new List<AttackPos>();

            Queue<Vector2Int> q = new Queue<Vector2Int>();

         //   cellPos.x *= AttackRange;
         //   cellPos.y *= AttackRange;

            q.Enqueue(cellPos);

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();


            while (q.Count > 0)
            {
                Vector2Int cur = q.Dequeue();

                for(int i = 0; i < 4; i++)
                {
                    int nx = cur.x + dx[i];
                    int ny = cur.y + dY[i];
                    Vector2Int next = new Vector2Int(nx, ny);

                    //이미 방문했다면 넘김
                    if (visited.Contains(next))
                        continue;

                    //공격 범위를 벗어났는지 체크. 
                    if (Math.Abs(ny - cellPos.y) > AttackRange)
                        continue;
                    if (Math.Abs(nx - cellPos.x) > AttackRange)
                        continue;

                    q.Enqueue(next);
                    visited.Add(next);

                    //전부 통과됬으니 넣음

                    AttackPos attkPos = new AttackPos
                    {
                        AttkPosX = next.x,
                        AttkPosY = next.y
                    };

                    attackList.Add(attkPos); 

                }
            }


            return attackList;
        }

        protected override Vector2Int GetDirFromNormal(Vector2Int normal)
        {
            return new Vector2Int(TargetPos.x, TargetPos.y);
        }
    }
}
