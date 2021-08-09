using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Spear : BaseWeapon
{
    Coroutine _coMove = null;
    bool _isRot = true; 

    void Awake()
    {
        Init();
    }
    protected override void Init()
    {
        base.Init();
        _attackRange = 1;
    }
    

    protected override void UpdateRotation()
    {
        if (_isRot == false)
            return;

        _q = Util.LookAt2D(transform.position, _targetPos , FacingDirection.UP);
        transform.rotation = _q;

    }


    public override void SkillEvent(List<AttackPos> attackList)
    {

        base.SkillEvent(attackList);
        //보여주기용 좌표 
        _coMove = StartCoroutine(CoMoveSpear(_ec, base._attackPos));
        _ec.transform.localPosition = _moveDir.normalized * 0.2f;
        _ec.transform.localRotation = _rot;
        _ec = null;

    }


    IEnumerator CoMoveSpear(EffectController ec , Vector3Int destPos,  float time = 0.2f)
    {     
        Vector3 newPos = new Vector3(destPos.x * 0.2f, destPos.y * 0.2f, 0);
        transform.localPosition = newPos;      
        yield return new WaitForSeconds(time);
        transform.localPosition = -newPos;
        _coMove = null;
   

    }
  
}
