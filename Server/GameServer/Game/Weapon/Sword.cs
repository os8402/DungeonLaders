using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Sword : Weapon
    {


        // 검은 자신을 기준으로 상 or 하 or 좌 or 우  중에서 
        // 2방향을 지정하고 
        // 그 후 2방향을 기준으로 대각선도 검사함  [즉 3방향] + [자기자신의 위치도 포함합니다]
        // 검 종류마다 공격 범위가 다를 것을 고려해 간단한 BFS로 구현
        //  
        //   me ㅁ      ㅁ ㅁ
        //   ㅁ ㅁ      ㅁ me
        //   이런 느낌으로 구현
        protected override List<AttackPos> CalcAttackRange(Vector2Int cellPos, int range)
        {
            List<AttackPos> attackList = new List<AttackPos>();

            int X, Y;

            X = (cellPos.x > 0 ? -1 : 1);
            Y = (cellPos.y < 0 ? 1 : -1);

            int[] dy = { 0, Y, Y };
            int[] dx = { X, 0, X };

            Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();
            Queue<Vector2Int> q = new Queue<Vector2Int>();

            AttackPos attkPos = new AttackPos { AttkPosX = cellPos.x, AttkPosY = cellPos.y };
            attackList.Add(attkPos);
            visited[cellPos] = true;

            q.Enqueue(cellPos);

            while (q.Count > 0)
            {
                Vector2Int cur = q.Dequeue();

                for (int i = 0; i < dx.Length; i++)
                {
                    int ny = cur.y + dy[i];
                    int nx = cur.x + dx[i];
                    Vector2Int nextPos = new Vector2Int(nx, ny);

                    //이미 방문했다면 넘김
                    if (visited.ContainsKey(nextPos))
                        continue;

                    //공격 범위를 벗어났는지 체크. 
                    if (Math.Abs(ny - cellPos.y) > range)
                        continue;
                    if (Math.Abs(nx - cellPos.x) > range)
                        continue;

                    q.Enqueue(nextPos);
                    visited[nextPos] = true;

                    attkPos.AttkPosX = nx;
                    attkPos.AttkPosY = ny; 

                    attackList.Add(attkPos);
                }
            }

            return attackList;
        }
     

        protected override Vector2Int GetDirFromNormal(Vector2Int pos)
        {
            int x = (pos.x > 0 ? 1 : -1) * _attackRange;
            int y = (pos.y > 0 ? 1 : -1) * _attackRange;

            return new Vector2Int(x, y);
        }


    }
}
