using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Arrow : Projectile
    {
        IJob _job;

        public override void Update()
        {
            if (Data == null || Data.projectile == null ||
                    Owner == null || Room == null)
                return;

            int tick = (int)(1000 / Data.projectile.speed);
            _job = Room.PushAfter(tick, Update);

            //TODO : 이동방법 구현
            int attackRange = Owner.EquipWeapon.AttackRange;

            Vector2Int dir = (CellPos - Owner.CellPos);
            int dist = dir.cellDistFromZero;
            if (dist > attackRange)
            {

                CancleJob();
                Room.Push(Room.LeaveGame, Id);
                return;
            }

            Vector2Int destPos = GetFrontCellPos();
            if (Room.Map.ApplyMove(this, destPos, checkObjects : false ,  collision: false))
            {
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(CellPos, movePacket);
            }
            else
            {
                Room.Push(Room.LeaveGame, Id);
            }

            GameObject target = Room.Map.Find(destPos);
            if (target != null && target.GetType() != Owner.GetType())
            {
                target.OnDamaged(this, Data.damage + Owner.TotalAttack);
                Room.Push(Room.LeaveGame, Id);
            }

            // 소멸
          


        }

        void CancleJob()
        {
            if (_job != null)
            {
                _job.Cancle = true;
                _job = null;
            }
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
