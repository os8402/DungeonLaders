using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        long _nextMoveTick = 0;

      

        public override void Update()
        {
            if (Owner == null || Room == null)
                return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            _nextMoveTick = Environment.TickCount64 + 20;
            //TODO : 이동방법 구현
            Vector2Int destPos = GetFrontCellPos();

            if (Room.Map.CanGo(destPos))
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.BroadCast(movePacket);

              //  Console.WriteLine("Move Arrow");
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if(target != null)
                {
                    //TODO : 피격판정
                }

                //소멸
                Room.LeaveGame(Id);
            }
        }
    }
}
