﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Arrow : Projectile
    {
       
        public override void Update()
        {
            if (WeaponData == null || WeaponData.projectile == null ||
                    Owner == null || Room == null)
                return;

            int tick = (int)(1000 / WeaponData.projectile.speed);
            Room.PushAfter(tick, Update);

            //TODO : 이동방법 구현
            Vector2Int destPos = GetFrontCellPos();

            if (Room.Map.ApplyMove(this , destPos , collision : false))
            {
    
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(CellPos, movePacket);

            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if(target != null && target != Owner)
                {
                    //TODO : 피격판정
                    target.OnDamaged(this, WeaponData.damage + Owner.TotalAttack);

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
