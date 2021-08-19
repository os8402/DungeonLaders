using GameServer.Data;
using GameServer.DB;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Monster : GameObject
    {
        public int TemplateId { get; private set; }

        public Monster()
        {
            ObjectType = GameObjectType.Monster;

        }
        public void Init(int templateId)
        {
            TemplateId = templateId;

            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(templateId, out monsterData);
            Stat.MergeFrom(monsterData.stat);
            Stat.Hp = monsterData.stat.MaxHp;
            State = ControllerState.Idle;

            EquipWeapon = ObjectManager.Instance.CreateObjectWeapon(-1);
            EquipWeapon.Owner = this;
            WeaponInfo.WeaponId = EquipWeapon.Id;

        }

        bool CanUseSkill(float dist , int x , int y )
        {
            bool canSkill = false;

            if (EquipWeapon == null || EquipWeapon.WeaponType == WeaponType.None)
                return false;

            int attackRange = EquipWeapon.AttackRange;

            //switch (EquipWeapon.Data.skillType)
            //{
            //    //일반 
            //    case SkillType.Normal:
            //        canSkill =
            //            dist <= attackRange + 1 && Math.Abs(x) <= attackRange && Math.Abs(y) <= attackRange;
            //        break;

            //    //투사체
            //    case SkillType.Projectile:

            //        canSkill =
            //            dist <= attackRange  && ((Math.Abs(x) <= attackRange && y == 0)  || (Math.Abs(y) <= attackRange && x == 0));
            //        break;
            //}
            canSkill =
                    dist <= attackRange + 1 && Math.Abs(x) <= attackRange && Math.Abs(y) <= attackRange;

            return canSkill;

        }

        //FSM [Finite State Machine} 
        IJob _job;
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

            if (Room != null)
                _job = Room.PushAfter(200, Update);

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

            Player target = Room.FindClosestPlayer(CellPos, _searchCellDist);
        

            if (target == null)
                return;

            if (target.State == ControllerState.Dead)
                return;

            _target = target;

            _target._checkDeadTarget -= CheckDeadTarget;
            _target._checkDeadTarget += CheckDeadTarget;

            State = ControllerState.Moving;
        }

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

            List<Vector2Int> path =   Room.Map.FindPath(CellPos, _target.CellPos , checkObjects : true);
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
            TargetPos = new Vector2Int(_target.CellPos.x, _target.CellPos.y);
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
           
            Room.Broadcast(CellPos, movePacket);
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
                    case SkillType.Normal:
                        //데미지 판정
                        _target.OnDamaged(this, EquipWeapon.Data.damage + TotalAttack);
                        break;

                    case SkillType.Projectile:
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
                skill.TargetInfo = TargetInfo;
                skill.AttackList.Add(EquipWeapon.AttackList);
                skill.AttackDir = EquipWeapon.AttackDir;

                Room.Broadcast(CellPos, skill);

                //스킬 쿨타임 적용
                int coolTick = (int)(1000 * EquipWeapon.Cooldown);
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
            if (_job != null)
            {
                _job.Cancle = true;
                _job = null;

            }
            _target = null;

            base.OnDead(attacker);

            GameObject owner = attacker.GetOwner();

            if(owner.ObjectType == GameObjectType.Player)
            {
                RewardData rewardData = GetRandomReward();
                if(rewardData != null)
                {
                    Player player = (Player)owner;

                    DbTransaction.RewardPlayer(player, rewardData , Room);
                    
                }
            }

        }

        void CheckDeadTarget(Player target)
        {
            if (target == _target)
                _target = null; 
        }

        RewardData GetRandomReward()
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId , out monsterData);

            int rand = new Random().Next(0, 101);

            //10 10 10 10 10 
            int sum = 0; 
            foreach(RewardData rewardData in monsterData.rewards)
            {
                sum += rewardData.probability; 
                if(rand <= sum)
                {
                    return rewardData; 
                }
            }

            return null; 
        }

    }
}
