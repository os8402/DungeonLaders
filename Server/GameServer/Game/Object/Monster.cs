﻿using GameServer.Data;
using GameServer.DB;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game
{
    public class Monster : GameObject
    {
        public int TemplateId { get; private set; }

        public int WeaponDamage { get { return EquipWeapon.Data.damage; } }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
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
            TotalHp = monsterData.stat.MaxHp;
            Stat.Hp = TotalHp;
            State = ControllerState.Idle;
            Info.Name = monsterData.name;
            Info.TeamId = (int)ObjectType << 24 | 0; 

            EquipWeapon = ObjectManager.Instance.CreateObjectWeapon();
   

        }

        bool CanUseSkill(float dist, int x, int y)
        {
            bool canSkill = false;

            if (EquipWeapon == null || EquipWeapon.WeaponType == WeaponType.None)
                return false;

            int attackRange = EquipWeapon.AttackRange;

            switch (EquipWeapon.Data.skillType)
            {
                //일반 
                case SkillType.Normal:
                    canSkill =
                        dist <= attackRange + 1 && Math.Abs(x) <= attackRange && Math.Abs(y) <= attackRange;
                    break;

                //투사체
                case SkillType.Projectile:

                    canSkill =
                        dist <= attackRange + 1 &&
                        ((Math.Abs(x) <= attackRange && y == 0) ||
                        (Math.Abs(y) <= attackRange && x == 0) ||
                        Math.Abs(x) == Math.Abs(y));
                    break;

                case SkillType.Magic:
                    canSkill =
                        dist <= attackRange + 1 && Math.Abs(x) <= attackRange && Math.Abs(y) <= attackRange;
                    break;
            }


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

            int moveTick = (int)(1000 / TotalSpeed);
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
            if (dist == 0 || dist > _chaseCellDist)
            {
                _target = null;
                State = ControllerState.Idle;
                BroadCastMove();
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: true);
            if (path.Count < 2 || path.Count > _chaseCellDist)
            {
                _target = null;
                State = ControllerState.Idle;
                BroadCastMove();
                return;
            }

            bool canUseSKill = CanUseSkill(dist, dir.x, dir.y);


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
            if (_coolTick == 0)
            {
                //유효 타겟인지
                if (_target == null || _target.Room != Room || _target.HP == 0)
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
                if (Dir != lookDir)
                {
                    Dir = lookDir;
                    BroadCastMove();

                }

                EquipWeapon.TargetPos = new Vector2Int(_target.CellPos.x, _target.CellPos.y);
                EquipWeapon.SkillEvent();

                TargetPos = new Vector2Int(_target.CellPos.x, _target.CellPos.y);


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



                switch (EquipWeapon.Data.skillType)
                {
                    case SkillType.Normal:
                        //데미지 판정
                        foreach(AttackPos pos in EquipWeapon.AttackList)
                        {
                            Vector2Int skillPos = new Vector2Int(PosInfo.PosX + pos.AttkPosX,
                          PosInfo.PosY + pos.AttkPosY);

                            GameObject target = Room.Map.Find(skillPos);

                            if (target != null)
                            {
                                //if (target.GetType() == typeof(Monster))
                                if (target.Info.TeamId == Info.TeamId)
                                    continue;

                                target.OnDamaged(this, TotalAttack);
                            }
                                                         
                        }
                        break;

                    case SkillType.Projectile:
                        // 투사체를 발사하는 원거리류는 생성만
                        Bow bow = EquipWeapon as Bow;
                        bow.ShootArrow();
                        break;

                    case SkillType.Magic:

                        // 마법

                        System.Random rand = new System.Random();
                        List<AttackPos> shupplePos = EquipWeapon.AttackList.OrderBy(num => rand.Next()).ToList();
                        EquipWeapon.AttackList = shupplePos; // 변

                        int t = (int)(EquipWeapon.Cooldown * 1000);

                        foreach (AttackPos pos in shupplePos)
                        {
                            Vector2Int skillPos = new Vector2Int(pos.AttkPosX, pos.AttkPosY);

                            GameObject target = Room.Map.Find(skillPos);

                            if (target != null)
                            {    
                                if (target.Info.TeamId == Info.TeamId)
                                    continue;

                                Room.PushAfter(t, target.OnDamaged, this, TotalAttack);

                            }
                             
                        }

             
                        break;

                }

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

            if (owner.ObjectType == GameObjectType.Player)
            {
                Player player = (Player)owner;

                SetPlayerExp(player);

                RewardData rewardData = GetRandomReward();

                if (rewardData != null)
                    DbTransaction.RewardPlayer(player, rewardData, Room);

            }

        }
        void SetPlayerExp(Player player)
        {
            if(player.Stat.Level < DataManager.StatDict.Count)
            {
                player.Exp += Stat.TotalExp;
                S_GetExp expPacket = new S_GetExp();
                expPacket.ObjectId = player.Id;
                expPacket.Exp = player.Exp;
                Room.Broadcast(player.CellPos, expPacket);

            }

        }

        void CheckDeadTarget(Player target)
        {
            if (target == _target)
                _target = null;
        }
        public void ClearTarget()
        {
            _target = null;
        }

        RewardData GetRandomReward()
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);


            // 자신의 무기도 드랍가능

            int weaponId = EquipWeapon.Id;

            RewardData rewardWeapon = new RewardData
            {
                itemId = weaponId,
                count = 1,
            };
       
            int rand = new Random().Next(0, 101);

   
            //10 10 10 10 10 
            int sum = 0;
            foreach (RewardData rewardData in monsterData.rewards)
            {
                sum += rewardData.probability;
                if (rand <= sum)
                {
                    return rewardData;
                }
            }

          

             return rewardWeapon;
        }

    }
}
