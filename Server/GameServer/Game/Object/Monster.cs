using GameServer.Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Monster : GameObject
    {
      
        public Monster()
        {
            ObjectType = GameObjectType.Monster;

            Stat.Level = 1;
            Stat.Hp = 100;
            Stat.MaxHp = 100;
            Stat.Speed = 5.0f;
            
            EquipWeapon = ObjectManager.Instance.CreateObjectWeapon(2);
            EquipWeapon.Owner = this;
            WeaponInfo.WeaponId = EquipWeapon.Id;

            State = ControllerState.Idle;
        
        }

        bool CanUseSkill(float dist , int x , int y )
        {
            bool canSkill = false;

            if (EquipWeapon == null || EquipWeapon.WeaponType == WeaponType.None)
                return false; 


            switch (EquipWeapon.Data.skillType)
            {
                //일반 
                case SkillType.SkillNormal:
                    canSkill =
                        dist <= _skillRange + 1 && Math.Abs(x) <= _skillRange && Math.Abs(y) <= _skillRange;
                    break;

                //투사체
                case SkillType.SkillProjectile:

                    canSkill =
                        dist <= _skillRange + 1 && ((Math.Abs(x) <= _skillRange && y == 0)  || (Math.Abs(y) <= _skillRange && x == 0));
                    break;
            }

            return canSkill;

        }

        //FSM [Finite State Machine} 
        public override void Update()
        {
            switch (State)
            {
                case ControllerState.Idle:
                    UpdateIdle();
                    break;
                case ControllerState.Moving:
                    UpdateMoving();
                    break;
                case ControllerState.Skill:
                    UpdateSkill();
                    break;
                case ControllerState.Dead:
                    UpdateDead();
                    break;
            }

        }

        Player _target; 

        int _searchCellDist = 10; 
        int _chaseCellDist = 10; 
        long _nextSearchTick = 0;
        protected virtual void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000;

            Player target =  Room.FindPlayer(p =>
            {
                Vector2Int dir =  p.CellPos - CellPos;
                return dir.cellDistFromZero <= _searchCellDist; 
            });

            if (target == null)
                return;

            if (target.State == ControllerState.Dead)
                return;

            _target = target;

            _target._checkDeadTarget -= CheckDeadTarget;
            _target._checkDeadTarget += CheckDeadTarget;

            State = ControllerState.Moving;
        }

        int _skillRange = 5; 
        long _nextMoveTick = 0;
        protected virtual void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;

            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            if (_target == null || _target.Room != Room)
            {
                _target = null;
                State = ControllerState.Idle;
                BroadCastMove();
                return; 
            }

            Vector2Int dir = (_target.CellPos - CellPos);
            int dist = dir.cellDistFromZero;
            if(dist == 0 || dist > _chaseCellDist)
            {
                _target = null;
                State = ControllerState.Idle;
                BroadCastMove();
                return;
            }

            List<Vector2Int> path =   Room.Map.FindPath(CellPos, _target.CellPos , checkObjects : false);
            if(path.Count < 2 || path.Count > _chaseCellDist)
            {
                _target = null;
                State = ControllerState.Idle;
                BroadCastMove();
                return;
            }

            bool canUseSKill = CanUseSkill(dist , dir.x , dir.y);


            if (canUseSKill)
            {
                _coolTick = 0; 
                State = ControllerState.Skill;
                return; 
            }

            //이동 

            Dir = GetDirState(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);
            BroadCastMove();

        }

        void BroadCastMove()
        {
            //패킷 전달
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.BroadCast(movePacket);
        }

        long _coolTick = 0; 
        protected virtual void UpdateSkill()
        {
            if(_coolTick == 0)
            {
                //유효 타겟인지
                if(_target == null || _target.Room != Room || _target.HP == 0)
                {
                    _target = null;
                    State = ControllerState.Moving;
                    BroadCastMove();
                    return;
                }
               
                // 스킬이 사용 가능한지
                Vector2Int dir = (_target.CellPos - CellPos);
                int dist = dir.cellDistFromZero;

                bool canUseSKill = CanUseSkill(dist, dir.x, dir.y);

                if (canUseSKill == false)
                {
                    State = ControllerState.Moving;
                    BroadCastMove();
                    return;
                }

                //타게팅 방향 주시
                DirState lookDir = GetDirState(dir);
                if(Dir != lookDir)
                {
                    Dir = lookDir;
                    BroadCastMove();

                }

                EquipWeapon.TargetPos = new Vector2Int(_target.CellPos.x, _target.CellPos.y);
                EquipWeapon.SkillEvent();

                TargetPos = new Vector2Int(_target.CellPos.x, _target.CellPos.y);

                switch (EquipWeapon.Data.skillType)
                {
                    case SkillType.SkillNormal:
                        //데미지 판정
                        _target.OnDamaged(this, EquipWeapon.Data.damage + Stat.Attack);
                        break;

                    case SkillType.SkillProjectile:
                        // 투사체를 발사하는 원거리류는 생성만
                        Bow bow = EquipWeapon as Bow;
                        bow.ShootArrow();
                        break;

                }

                //스킬 사용 broadCast 

                S_Skill skill = new S_Skill
                {
                    AttackDir = new AttackPos(),
                    TargetInfo = new TargetInfo() 
                };

                skill.ObjectId = Id;
                skill.TargetInfo = Target;
                skill.AttackList.Add(EquipWeapon.AttackList);
                skill.AttackDir = EquipWeapon.AttackDir;

                Room.BroadCast(skill);

                //스킬 쿨타임 적용
                int coolTick = (int)(1000 * EquipWeapon.Data.cooldown);
                _coolTick = Environment.TickCount64 + coolTick;

                      

            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }
        protected virtual void UpdateDead()
        {

        }
        public override void OnDead(GameObject attacker)
        {
            _target = null;

            base.OnDead(attacker);
        }

        void CheckDeadTarget(Player target)
        {
            if (target == _target)
                _target = null; 
        }
    }
}
