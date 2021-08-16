using GameServer.Data;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; set; } = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId);
            Monster monster = ObjectManager.Instance.Add<Monster>();

            monster.CellPos = new Vector2Int(3, 3);
            EnterGame(monster);

            

        }
      

        public void Update()
        {

            foreach (Monster monster in _monsters.Values)
            {
                monster.Update();
            }
            foreach (Projectile projectile in _projectiles.Values)
            {
                projectile.Update();
            }

            Flush();
        }


        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeId(gameObject.Id);


            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

                //본인한테 전송
                {

                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            spawnPacket.Objects.Add(p.Info);
                    }

                    foreach (Monster m in _monsters.Values)
                        spawnPacket.Objects.Add(m.Info);

                    foreach (Projectile p in _projectiles.Values)
                        spawnPacket.Objects.Add(p.Info);


                    player.Session.Send(spawnPacket);
                }

            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;
            }

            //타인한테도 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != gameObject.Id)
                        p.Session.Send(spawnPacket);
                }

            }



        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeId(objectId);


            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                player.OnLeaveGame();
                Map.ApplyLeave(player);
                player.Room = null;

                //본인
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.Remove(objectId, out monster) == false)
                    return;
          
                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;

                if (_projectiles.Remove(objectId, out projectile) == false)
                    return;

                projectile.Room = null;

            }

            //타인
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);

                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }
            }


        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            // TODO : 검증 
            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            //다른 좌표로 이동할 경우 갈 수 있는지 
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;
            }

            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.Target = movePosInfo.Target;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            //다른 플레이어한테도 알려준다 
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;
            BroadCast(resMovePacket);


        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;


            ObjectInfo info = player.Info;

            //TODO : 스킬 사용 가능 여부

            EquipWeapon weapon = player.EquipWeapon;
            int x = skillPacket.AttackPos.AttkPosX;
            int y = skillPacket.AttackPos.AttkPosY;
            weapon.TargetPos = new Vector2Int(x, y);
            weapon.SkillEvent();


            //1.공격 범위 
            //2.공격 방향
            //3.타겟 정보
            //모아서 클라에 보냄

            S_Skill skill = new S_Skill();

            skill.ObjectId = info.ObjectId;
            skill.AttackList.Add(weapon.AttackList);
            skill.AttackDir = weapon.AttackDir;
            skill.TargetInfo = skillPacket.TargetInfo;

            BroadCast(skill);
    
            switch (weapon.Data.skillType)
            {
                case SkillType.SkillNormal:

                    // TODO : 데미지 판정
                    foreach (AttackPos pos in weapon.AttackList)
                    {
                        Vector2Int skillPos = new Vector2Int(info.PosInfo.PosX + pos.AttkPosX,
                           info.PosInfo.PosY + pos.AttkPosY);

                        GameObject target = Map.Find(skillPos);

                        if (target != null)
                        {
                            target.OnDamaged(target, weapon.Data.damage + player.Stat.Attack);
                        }
                    }
                    break;

                case SkillType.SkillProjectile:

                    // 투사체를 발사하는 원거리류는 생성만
                    Bow bow = weapon as Bow;
                    bow.ShootArrow();
                    break;
            }


        }

        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        public void BroadCast(IMessage packet)
        {

            foreach (Player p in _players.Values)
            {
                p.Session.Send(packet);
            }

        }

    }
}
