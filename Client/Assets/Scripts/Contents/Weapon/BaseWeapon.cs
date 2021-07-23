using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    protected int id;
    protected SpriteRenderer _spriteRenderer;
    protected CreatureController _owner;

    protected Vector3 _targetPos; 
    protected float _attackRange = 0;
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
            _targetPos = _owner.GetComponent<MonsterController>().Target.position;
        
    }

    void Update()
    {
        GetControllerType();
        UpdateRotation();
    }

    protected abstract void UpdateRotation();
    public abstract void SkillEvent();

}
