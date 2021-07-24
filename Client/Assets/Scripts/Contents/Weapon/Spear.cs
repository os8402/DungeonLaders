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
        _attackRange = 3;
    }

    protected override void UpdateRotation()
    {
        _q = Util.LookAt2D(transform.position, _targetPos , FacingDirection.UP);
        transform.rotation = _q;

    }
    public override void SkillEvent()
    {

    }

    protected override List<Vector3Int> GetAttackRange(Vector3Int cellPos, int dirX, int dirY, int range)
    {
        List<Vector3Int> attackList = new List<Vector3Int>();

        return attackList;
    }
}
