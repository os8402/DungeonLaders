using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }
        public GameRoom Room { get; set; }

        public ObjectInfo Info { get; set; } = new ObjectInfo();
    
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        public WeaponInfo WeaponInfo { get; private set; } = new WeaponInfo();
        
        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.WeaponInfo = WeaponInfo;
           
        }

        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }
            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;

            }
        }

        public DirState Dir
        {
            get { return PosInfo.Dir; }
            set { PosInfo.Dir = value; }
           
        }

        public DirState GetDirState(int posX, int posY)
        {
            DirState state = DirState.Up;

            if (posX == 0 && posY == 1)
                state = DirState.Up;
            else if (posX == 0 && posY == -1)
                state = DirState.Down;
            else if (posX == -1 && posY == 0)
                state = DirState.Left;
            else if (posX == 1 && posY == 0)
                state = DirState.Right;
            else if (posX == -1 && posY == 1)
                state = DirState.UpLeft;
            else if (posX == -1 && posY == -1)
                state = DirState.DownLeft;
            else if (posX == 1 && posY == -1)
                state = DirState.DownRight;
            else if (posX == 1 && posY == 1)
                state = DirState.UpRight;

            return state;
        }

        public Weapon Weapon { get; set; }
    }
}
