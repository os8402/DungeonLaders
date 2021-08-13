using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Bow : EquipWeapon
{
    private Animator _animator;

    void Awake()
    {
        Init();

    }
    protected override void Init()
    {
        base.Init();
        _attackRange = 1;
        _animator = GetComponent<Animator>(); 
    }

    protected override void UpdateRotation()
    {
        _q = Util.LookAt2D(_targetPos , transform.position , FacingDirection.LEFT);
        transform.rotation = _q;
    }
    public override void SkillEvent(S_Skill skillPacket)
    {

        base.SkillEvent(skillPacket);
        //투사체 계열 무기들은 애니메이션만 적용 
        _animator.Play("Bow");

    }
    
}
