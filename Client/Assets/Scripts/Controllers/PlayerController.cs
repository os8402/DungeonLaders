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
  
  

    protected override void UpdateRotation()
    {
        Quaternion q = Util.RotateDir2D(transform.position, TargetPos, true);

        if (q.z > Quaternion.identity.z) // 오른쪽
        {
            Dir = 1;
        }
        else if (q.z < Quaternion.identity.z)// 왼쪽
        {
            Dir = -1;
        }

        else return;

    }
   


   
    IEnumerator CoSkillAttack(float time)
    {
        _skillEvent?.Invoke();
        yield return new WaitForSeconds(time);
        _coSkill = null;
 

    }

    public override void OnDead(GameObject attacker)
    {

        base.OnDead(attacker);
        deadTargetEvent.Invoke(this);
        Managers.Input.Clear();
        
    }


}
