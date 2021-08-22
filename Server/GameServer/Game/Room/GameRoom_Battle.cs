using GameServer.Data;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public partial class GameRoom : JobSerializer
    {


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
            Broadcast(player.CellPos, resMovePacket);


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

            Broadcast(player.CellPos, skill);

            switch (weapon.Data.skillType)
            {
                case SkillType.Normal:

                    // TODO : 데미지 판정
                    foreach (AttackPos pos in weapon.AttackList)
                    {
                        Vector2Int skillPos = new Vector2Int(info.PosInfo.PosX + pos.AttkPosX,
                           info.PosInfo.PosY + pos.AttkPosY);

                        GameObject target = Map.Find(skillPos);

                        if (target != null)
                        {
                            target.OnDamaged(player, weapon.Data.damage + player.Stat.Attack);
                        }
                    }
                    break;

                case SkillType.Projectile:

                    // 투사체를 발사하는 원거리류는 생성만
                    Bow bow = weapon as Bow;
                    bow.ShootArrow();
                    break;
            }


        }

        public void HandleLevelUp(Player player, C_LevelUp upPacket)
        {
            if (player == null)
                return;

            player.HandleLevelUp(upPacket);
        }

    }
}
