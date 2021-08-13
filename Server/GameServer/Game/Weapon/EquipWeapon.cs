using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using GameServer.Data;
using GameServer.Game;

public abstract class EquipWeapon
{
    public Weapon WeaponData { get; set; }

    public int Id { get; set; }

    public Weapons WeaponType { get; set; }

    //소유자 
    public Player Owner { get; set; }
    public Vector2Int TargetPos { get; set; }  //플레이어 -> 마우스 / 몬스터 -> 플레이어
    protected int _attackRange = 1; // 공격범위

    public int AttackRange
    {
        get { return _attackRange; }
    }

    public List<AttackPos> AttackList { get; set; }
    public AttackPos AttackDir { get; set; }


    //무기별 공격범위 계산
    protected abstract List<AttackPos> CalcAttackRange(Vector2Int cellPos, int range);
    //무기 공격 방향 처리
    protected abstract Vector2Int GetDirFromNormal(Vector2Int normal);

    //무기별 스킬 이벤트
    public void SkillEvent()
    {
        if(AttackList != null)
            AttackList.Clear();

        Vector2Int dir = TargetPos - Owner.CellPos;
        int dirX = (dir.x != 0) ? dir.x / Math.Abs(dir.x) : 0;
        int dirY = (dir.y != 0) ? dir.y / Math.Abs(dir.y) : 0;
      
        Vector2Int normal = new Vector2Int(dirX , dirY);

        Vector2Int attackPos = GetDirFromNormal(normal);   
    
        //공격 방향
        AttackDir = new AttackPos
        {
            AttkPosX = attackPos.x,
            AttkPosY = attackPos.y
        };

        //공격 범위
        AttackList = CalcAttackRange(attackPos, _attackRange);
        //자기 자신도 포함됬을 경우 제거 [안전 장치] 
        AttackList.Remove(new AttackPos { AttkPosX = 0, AttkPosY = 0 });
        
    }

}
