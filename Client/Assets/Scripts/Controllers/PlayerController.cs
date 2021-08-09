using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{

    [SerializeField]
    protected bool _moveKeyPressed = false;
    protected bool _mouseKeyPressed = false;
    //죽었을 때 이벤트 날려서 물어봐야 함 [바로 사라지지않기 때문에] 
    public Action<PlayerController> deadTargetEvent = null;

    protected override void Init()
    {
        MaxHp = 1000;
        base.Init();
    }



    void Start()
    {
        Init();
    }
  
    public void UseSkill(List<AttackPos> attackList)
    {
        if (_coSkill != null)
            return;
        // 스킬 공격 
        _coSkill = StartCoroutine(CoSkillAttack(0.2f , attackList));

    }
    protected virtual void CheckUpdatedFlag()
    {
      
    }

    IEnumerator CoSkillAttack(float time , List<AttackPos> attackList)
    {
        _skillEvent?.Invoke(attackList);
        yield return new WaitForSeconds(time);
        _coSkill = null;
        CheckUpdatedFlag();


    }

    public override void OnDead(GameObject attacker)
    {

        base.OnDead(attacker);
        deadTargetEvent.Invoke(this);
        Managers.Input.Clear();
        
    }


}
