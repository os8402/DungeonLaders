using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        Map _map = new Map();

        public void Init(int mapId)
        {
            _map.LoadMap(mapId);
        }

        public void EnterGame(Player newPlayer)
        {
            if (newPlayer == null)
                return;

            lock(_lock)
            {
                _players.Add(newPlayer.Info.PlayerId, newPlayer);
                newPlayer.Room = this;

                //본인한테 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (newPlayer != p)
                            spawnPacket.Players.Add(p.Info);
                    }

                    newPlayer.Session.Send(spawnPacket);
                }
                //타인한테도 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach(Player p in _players.Values)
                    {
                        if (newPlayer != p)
                            p.Session.Send(spawnPacket);
                    }

                }
            }

          
        }

        public void LeaveGame(int playerId)
        {
            lock(_lock)
            {
                Player player = null;
                if (_players.Remove(playerId, out player) == false)
                    return;

        
                player.Room = null;


                //본인
                { 
                   S_LeaveGame leavePacket = new S_LeaveGame();
                   player.Session.Send(leavePacket);
                }

                //타인
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.Info.PlayerId);

                    foreach(Player p in _players.Values)
                    {
                        if(player != p)
                           p.Session.Send(despawnPacket);
                    }
                }

            }
        }

        public void HandleMove(Player player , C_Move movePacket)
        {
            if (player == null)
                return; 

            lock(_lock)
            {
                // TODO : 검증 
                PositionInfo movePosInfo = movePacket.PosInfo;
                PlayerInfo info = player.Info;
                
                //다른 좌표로 이동할 경우 갈 수 있는지 
                if(movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    if (_map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                        return;
                }

                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.Dir = movePosInfo.Dir;
                _map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                //다른 플레이어한테도 알려준다 
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = info.PlayerId;
                resMovePacket.PosInfo = movePacket.PosInfo;    
                BroadCast(resMovePacket);
            }

        }

        public void HandleSkill(Player player , C_Skill skillPacket)
        {
            if (player == null)
                return;

            lock(_lock)
            {
                PlayerInfo info = player.Info;
                // if (info.PosInfo.State != ControllerState.Idle)
                //  return;

                //TODO : 스킬 사용 가능 여부

                //통과
                //info.PosInfo.State = ControllerState.Skill;

                Weapon weapon = player.Weapon;
                int x = skillPacket.AttackPos.AttkPosX;
                int y = skillPacket.AttackPos.AttkPosY;
                weapon.TargetPos = new Vector2Int(x , y);
                weapon.SkillEvent();



                S_Skill skill = new S_Skill();
           
                skill.PlayerId = info.PlayerId;
                skill.AttackList.Add(weapon.AttackList);
                skill.AttackDir = weapon.AttackDir;
                skill.TargetInfo = skillPacket.TargetInfo;
               
                BroadCast(skill);
                

                // TODO : 데미지 판정
            }
        }

        public void BroadCast(IMessage packet)
        {
            lock(_lock)
            {
                foreach(Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }

    }
}
