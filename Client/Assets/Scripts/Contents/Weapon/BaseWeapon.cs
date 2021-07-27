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

    protected Vector3 _targetPos; 
    protected int _attackRange = 1;
    public int AttackRange { get { return _attackRange; } }
    protected Quaternion _q = new Quaternion();

    protected Vector3 _moveDir;
    protected Quaternion _rot;

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

        if (_owner.GetType() == typeof(PlayerController))      
           _targetPos =  _owner.GetComponent<PlayerController>().Cam.MousePos;

        
        else if (_owner.GetType() == typeof(MonsterController))
        {
            PlayerController target = _owner.GetComponent<MonsterController>().Target;
            if (target == null)
                return;

            _targetPos = _owner.GetComponent<MonsterController>().Target.transform.position;
        }
          
        
    }

    void Update()
    {
        GetControllerType();
        UpdateRotation();
    }
    //무기별 공격 범위
    protected abstract List<Vector3Int> GetAttackRange(Vector3Int cellPos , int range);

    //무기별 회전 처리
    protected abstract void UpdateRotation();

    //무기 공격 방향 처리
    protected abstract int GetDirFromNormal(float num);

    //무기별 스킬 이벤트
    public virtual void SkillEvent()
    {
        string weaponName = this.GetType().Name;
        GameObject go = Managers.Resource.Instantiate($"Effect/{weaponName}/{weaponName}_Eff_{id.ToString("000")}");
        _ec = go.GetComponent<EffectController>();

         _moveDir = _targetPos - _owner.transform.position;
        _rot = Util.LookAt2D(_targetPos, transform.position, FacingDirection.LEFT);

        _ec.transform.parent = transform.parent;

        //실제 좌표
        _ec.Pos = _owner.Pos;
        //소유자 등록 [누가 공격했는지 전달해줘야 함 ] 
        _ec.Owner = _owner;

        int dirX = GetDirFromNormal(_moveDir.normalized.x);
        int dirY = GetDirFromNormal(_moveDir.normalized.y);

        _attackPos = new Vector3Int(dirX, dirY, 0);

        //공격 범위
        List<Vector3Int> attackList = GetAttackRange(_attackPos, _attackRange);
        _ec.AttackList = attackList;
    }

}
