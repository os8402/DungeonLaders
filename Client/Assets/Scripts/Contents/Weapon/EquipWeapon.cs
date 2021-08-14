using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public abstract class EquipWeapon : MonoBehaviour
{
    protected int id;
    public int Id { get; set; }
    protected SpriteRenderer _spriteRenderer;
    protected CreatureController _owner;
    protected Vector3 _targetPos; 
    protected int _attackRange = 1;
    public int AttackRange { get { return _attackRange; } }
    protected Quaternion _q = new Quaternion();
    protected Quaternion _rot;
    protected Vector3 _moveDir; 

    protected AttackPos _attackDir;
    protected EffectController _ec = null; //후처리용



    void Awake()
    {
        Init();

    }

    protected virtual void Init()
    {
 
        _spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.name = GetType().Name;
        _owner = GetComponentInParent<CreatureController>();
        id = 1;

    }

    void GetControllerType()
    {
        //플레이어 몬스터 둘 다 무기를 들 수 있음
 

        if (_owner == null)
            Init();

        _targetPos = _owner.TargetPos;

    }

    void Update()
    {
        GetControllerType();
        UpdateRotation();
    }

    //무기별 회전 처리
    protected abstract void UpdateRotation();

    //무기별 스킬 이벤트
    public virtual void SkillEvent(S_Skill skillPacket)
    {
        List<AttackPos> attackList = skillPacket.AttackList.ToList();
        _attackDir = skillPacket.AttackDir;

        //프레임 시간 차로 바로 갱신이 안 되서 여기서도 적용
        float targetX = skillPacket.TargetInfo.TargetX;
        float targetY = skillPacket.TargetInfo.TargetY;
        _targetPos = new Vector3(targetX, targetY);

        string weaponName = this.GetType().Name;

        _moveDir = new Vector3(_attackDir.AttkPosX, _attackDir.AttkPosY);
        _rot = Util.LookAt2D(_moveDir, Vector2.zero, FacingDirection.LEFT);

        //이펙트 출력
        GameObject go = Managers.Resource.Instantiate($"Effect/{weaponName}/{weaponName}_Eff_{id.ToString("000")}");
        _ec = go.GetComponent<EffectController>();
        _ec.transform.parent = transform.parent;
        ////실제 공격범위는 서버에서 처리 예정
        _ec.AttackList = attackList;

        if (_owner == null)
            return;

        //실제 좌표 + 소유자 등록 [누가 공격했는지 전달해줘야 함 ] 
        _ec.CellPos = _owner.CellPos;
        _ec.Owner = _owner;
   

    }

}
