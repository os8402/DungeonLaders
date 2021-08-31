using GameServer.Game;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public struct Pos
{
    public Pos(int y, int x) { Y = y; X = x; }
    public int Y;
    public int X;

    public static bool operator ==(Pos lhs, Pos rhs)
    {
        return lhs.Y == rhs.Y && lhs.X == rhs.X;
    }
    public static bool operator !=(Pos lhs, Pos rhs)
    {
        return !(lhs == rhs);
    }
    public override bool Equals(object obj)
    {
        return (Pos)obj == this;
    }
    public override int GetHashCode()
    {
        long value = (Y << 32) | X;
        return value.GetHashCode();
    }
    public override string ToString()
    {
        return base.ToString();
    }
}

public struct PQNode : IComparable<PQNode>
{
    public int F;
    public int G;
    public int Y;
    public int X;

    public int CompareTo(PQNode other)
    {
        if (F == other.F)
            return 0;
        return F < other.F ? 1 : -1;
    }
}

public struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x, int y) { this.x = x; this.y = y; }

    public static Vector2Int up { get { return new Vector2Int(0, 1); } }
    public static Vector2Int down { get { return new Vector2Int(0, -1); } }
    public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
    public static Vector2Int right { get { return new Vector2Int(1, 0); } }

    public static Vector2Int operator +(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x + b.x, a.y + b.y);
    }

    public static Vector2Int operator -(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x - b.x, a.y - b.y);
    }

    public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }
    public int sqrMagnitude { get { return (x * x + y * y); } }
    public int cellDistFromZero { get { return Math.Abs(x) + Math.Abs(y); } }
}


public class Map
{

    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }

    public int SizeX { get { return MaxX - MinX + 1; } }
    public int SizeY { get { return MaxY - MinY + 1; } }

    bool[,] _collision;
    GameObject[,] _objects;

    //bool[,] closed;
    //int[,] open;
    //Pos[,] parent; // 경로추적을 위해서

    public int CellDistFromZero(int x, int y)
    {
        return Math.Abs(x) + Math.Abs(y);
    }



    public bool OutOfMap(Vector2Int cellPos)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return true;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return true;

        return false;
    }

    public bool CanGo(Vector2Int cellPos, bool checkObjects = true)
    {

        if (OutOfMap(cellPos))
            return false;

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        return !_collision[y, x] && (!checkObjects || _objects[y, x] == null);
    }

    public GameObject Find(Vector2Int cellPos)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return null;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return null;

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        return _objects[y, x];
    }

    public bool ApplyLeave(GameObject gameObject)
    {
        if (gameObject.Room == null)
            return false;
        if (gameObject.Room.Map != this)
            return false;

        PositionInfo posInfo = gameObject.PosInfo;
        if (OutOfMap(gameObject.CellPos))
            return false;

        //zone

        Zone zone = gameObject.Room.GetZone(gameObject.CellPos);
        zone.Remove(gameObject);

        {
            int x = posInfo.PosX - MinX;
            int y = MaxY - posInfo.PosY;
            if (_objects[y, x] == gameObject)
                _objects[y, x] = null;
        }

        return true;

    }
    public bool ApplyMove(GameObject gameObject, Vector2Int dest, bool checkObjects = true, bool collision = true , bool canGo = true)
    {
   
        if (gameObject.Room == null)
            return false;
        if (gameObject.Room.Map != this)
            return false;

        PositionInfo posInfo = gameObject.PosInfo;
        if(canGo)
        {

            if (CanGo(dest, checkObjects) == false)
                return false;

        }
        else
        {
            if (OutOfMap(dest))
                return false;
        }
    
        if (collision)
        {
            {
                int x = posInfo.PosX - MinX;
                int y = MaxY - posInfo.PosY;
                if (_objects[y, x] == gameObject)
                    _objects[y, x] = null;
            }

            {
                int x = dest.x - MinX;
                int y = MaxY - dest.y;
                _objects[y, x] = gameObject;
            }
        }

        GameObjectType type = ObjectManager.GetObjectTypeId(gameObject.Id);

        if (type == GameObjectType.Player)
        {
            Player player = (Player)gameObject;

            //존 갱신
            Zone now = gameObject.Room.GetZone(gameObject.CellPos);
            Zone after = gameObject.Room.GetZone(dest);
            if (now != after)
            {
                now.Players.Remove(player);
                after.Players.Add(player);
            }

        }
        else if (type == GameObjectType.Monster)
        {
            Monster monster = (Monster)gameObject;

            //존 갱신
            Zone now = gameObject.Room.GetZone(gameObject.CellPos);
            Zone after = gameObject.Room.GetZone(dest);
            if (now != after)
            {
                now.Monsters.Remove(monster);
                after.Monsters.Add(monster);
            }

        }
        else if (type == GameObjectType.Projectile)
        {
            Projectile projectile = (Projectile)gameObject;

            //존 갱신
            Zone now = gameObject.Room.GetZone(gameObject.CellPos);
            Zone after = gameObject.Room.GetZone(dest);
            if (now != after)
            {
                now.Projectiles.Remove(projectile);
                after.Projectiles.Add(projectile);
            }

        }

        posInfo.PosX = dest.x;
        posInfo.PosY = dest.y;
        return true;
    }

    public void LoadMap(int mapId, string pathPrefix = "../../../../../../Common/MapData")
    {

        string mapName = "Common_Map_" + mapId.ToString("000");

        string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
        StringReader reader = new StringReader(text);

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new bool[yCount, xCount];
        _objects = new GameObject[yCount, xCount];

        for (int y = 0; y < yCount; y++)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; x++)
                _collision[y, x] = (line[x] == '1' ? true : false);

        }
    }


    #region A* PathFinding

    // U D L R UL DL DR UR
    int[] _deltaY = new int[] { 1, -1, 0, 0, -1, 1, 1, -1 };
    int[] _deltaX = new int[] { 0, 0, -1, 1, -1, -1, 1, 1 };
    // 상하좌우 cost = 1  , 대각선 cost = 1.4f  , 실수는 연산부하가 있으므로 10을 곱해서 정수로 계산하였음 
    int[] _cost = new int[] { 10, 10, 10, 10, 14, 14, 14, 14 };

    public List<Vector2Int> FindPath(Vector2Int startCellPos, Vector2Int destCellPos, bool checkObjects = true, int maxDist = 10 , bool canGo = true)
    {
        // 점수 매기기
        // F = G + H
        // F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
        // G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
        // H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

        // (y, x) 이미 방문했는지 여부 (방문 = closed 상태
        HashSet<Pos> closeList = new HashSet<Pos>(); // CloseList

        // (y, x) 가는 길을 한 번이라도 발견했는지
        // 발견X => MaxValue
        // 발견O => F = G + H
        Dictionary<Pos, int> openList = new Dictionary<Pos, int>(); // OpenList
        Dictionary<Pos, Pos> parent = new Dictionary<Pos, Pos>();

        // 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

        // CellPos -> ArrayPos
        Pos pos = Cell2Pos(startCellPos);
        Pos dest = Cell2Pos(destCellPos);

        // 시작점 발견 (예약 진행)
        openList.Add(pos, 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)));

        pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
        parent.Add(pos, pos);

        while (pq.Count > 0)
        {
            // 제일 좋은 후보를 찾는다
            PQNode pqNode = pq.Pop();
            Pos node = new Pos(pqNode.Y, pqNode.X);
            // 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
            if (closeList.Contains(node))
                continue;

            // 방문한다
            closeList.Add(node);
            // 목적지 도착했으면 바로 종료
            if (node.Y == dest.Y && node.X == dest.X)
                break;

            // 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
            for (int i = 0; i < _deltaY.Length; i++)
            {
                Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

                //너무 멀어도 스킵
                if (Math.Abs(pos.Y - next.Y) + Math.Abs(pos.X - next.X) > maxDist)
                    continue;


                // 유효 범위를 벗어났으면 스킵
                // 벽으로 막혀서 갈 수 없으면 스킵
                if (next.Y != dest.Y || next.X != dest.X)
                {
                    if(canGo)
                    {
                        if (CanGo(Pos2Cell(next), checkObjects) == false) // CellPos
                            continue;

                    }
                 
                }

                // 이미 방문한 곳이면 스킵
                if (closeList.Contains(next))
                    continue;


                // 비용 계산
                int g = pqNode.G + _cost[i];
                int h = _deltaY[i] * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                // 다른 경로에서 더 빠른 길 이미 찾았으면 스킵

                int value = 0;
                if (openList.TryGetValue(next, out value) == false)
                    value = Int32.MaxValue;

                if (value < g + h)
                    continue;

                // 예약 진행			
                if (openList.TryAdd(next, g + h) == false)
                    openList[next] = g + h;


                pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });

                if (parent.TryAdd(next, node) == false)
                    parent[next] = node;

            }
        }

        return CalcCellPathFromParent(parent, dest);
    }

    List<Vector2Int> CalcCellPathFromParent(Dictionary<Pos, Pos> parent, Pos dest)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        if(parent.ContainsKey(dest) == false)
        {
            Pos best = new Pos();
            int bestDist = Int32.MaxValue;

            foreach(Pos pos in parent.Keys)
            {
                int dist = Math.Abs(dest.X - pos.X) + Math.Abs(dest.Y - pos.Y);

                //제일 우수한 후보
                if(dist < bestDist)
                {
                    best = pos;
                    bestDist = dist;
                }
               
            }

            dest = best;

        }

        {
            Pos pos = dest;

            while (parent[pos] != pos)
            {
                cells.Add(Pos2Cell(pos));
                pos = parent[pos];
            }
            cells.Add(Pos2Cell(pos));
            cells.Reverse();
        }

        return cells;
    }

    public Pos Cell2Pos(Vector2Int cell)
    {
        // CellPos -> ArrayPos
        return new Pos(MaxY - cell.y, cell.x - MinX);
    }

    public Vector2Int Pos2Cell(Pos pos)
    {
        // ArrayPos -> CellPos
        return new Vector2Int(pos.X + MinX, MaxY - pos.Y);
    }

    #endregion
}
