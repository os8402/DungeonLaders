using GameServer.Game;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;



public abstract class Weapon
{
 

    protected int id;
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

    //무기별 공격범위 계산
    protected abstract List<AttackPos> CalcAttackRange(Vector2Int cellPos, int range);
    //무기 공격 방향 처리
    protected abstract Vector2Int GetDirFromNormal(Vector2Int num);

    //무기별 스킬 이벤트
    public List<AttackPos> SkillEvent()
    {
 
        Vector2Int dir = TargetPos - Owner.CellPos;
        Vector2Int normal = GetDirFromNormal(dir);
        Vector2Int _attackPos = new Vector2Int(normal.x, normal.y);

        //공격 범위
        return CalcAttackRange(_attackPos, _attackRange);
    }

}
