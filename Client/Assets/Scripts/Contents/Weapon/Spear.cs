using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Spear : BaseWeapon
{
    void Awake()
    {
        Init();
    }
    protected override void Init()
    {
        base.Init();
        _attackRange = 2.0f;
    }

    protected override void UpdateRotation()
    {
        _q = Util.LookAt2D(transform.position, _targetPos , FacingDirection.UP);
        transform.rotation = _q;

    }
    public override void SkillEvent()
    {

    }


}
