using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public abstract class BaseWeapon : MonoBehaviour
{
    protected int id;
    public int Id { get; set; }
    protected SpriteRenderer _spriteRenderer;
    [SerializeField]
    protected CreatureController _owner;
    [SerializeField]
    protected Vector3 _targetPos; 
    protected int _attackRange = 1;
    public int AttackRange { get { return _attackRange; } }
    protected Quaternion _q = new Quaternion();
    protected Quaternion _rot;
    protected Vector3 _moveDir; 

    protected Vector3Int _attackPos;
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
    public virtual void SkillEvent(List<AttackPos> attackList)
    {
        string weaponName = this.GetType().Name;
        _moveDir = _targetPos - _owner.transform.position;

        //이펙트 출력
        GameObject go = Managers.Resource.Instantiate($"Effect/{weaponName}/{weaponName}_Eff_{id.ToString("000")}");
        _ec = go.GetComponent<EffectController>();
        _rot = Util.LookAt2D(_targetPos, transform.position, FacingDirection.LEFT);
        _ec.transform.parent = transform.parent;
        //실제 좌표 + 소유자 등록 [누가 공격했는지 전달해줘야 함 ] 
        _ec.CellPos = _owner.CellPos;
        _ec.Owner = _owner;
        ////실제 공격범위는 서버에서 처리 예정
        _ec.AttackList = attackList;

    }

}
