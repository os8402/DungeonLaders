using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Player
    {
        public PlayerInfo Info { get; set; } = new PlayerInfo()
        { 
            PosInfo = new PositionInfo(),
            WeaponInfo = new WeaponInfo()
   
        }; 
        public GameRoom Room { get; set;}
        public ClientSession Session { get; set; }

        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(Info.PosInfo.PosX, Info.PosInfo.PosY);
            }
            set
            {
                Info.PosInfo.PosX = value.x;
                Info.PosInfo.PosY = value.y;
        
            }
        }

        public Weapon Weapon { get; set; }


    }
}
