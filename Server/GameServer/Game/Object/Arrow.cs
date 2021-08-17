using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Arrow : Projectile
    {
       

        long _nextMoveTick = 0;

        public override void Update()
        {
            if (WeaponData == null || WeaponData.projectile == null ||
                    Owner == null || Room == null)
                return;


            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long)(1000 / WeaponData.projectile.speed);

            _nextMoveTick = Environment.TickCount64 + tick;
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
                if(target != null && target != Owner)
                {
                    //TODO : 피격판정
                    target.OnDamaged(this, WeaponData.damage + Owner.Stat.Attack);

                }
                //소멸
                Room.Push(Room.LeaveGame, Id);
            }
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
