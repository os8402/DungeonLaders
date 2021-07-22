using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    protected SpriteRenderer _spriteRenderer;
    protected PlayerController _owner;
    protected float _attackRange = 0;
    protected Quaternion _q = new Quaternion();

    void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _owner = GetComponentInParent<PlayerController>();
        gameObject.name = GetType().Name;
    }

    protected abstract void UpdateRotation();
    public abstract void SkillEvent();

}
