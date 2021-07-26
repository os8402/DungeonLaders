using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Awake()
    {
        Init();

    }

    protected virtual void Init()
    {
 
        _spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.name = GetType().Name;
        _owner = GetComponentInParent<CreatureController>();

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
    //무기별 스킬 이벤트
    public abstract void SkillEvent();

}
