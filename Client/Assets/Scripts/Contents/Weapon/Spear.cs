using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Spear : EquipWeapon
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

        _q = Util.LookAt2D(transform.position , _targetPos, FacingDirection.UP);
        transform.rotation = _q;

    }


    public override void SkillEvent(S_Skill skillPacket)
    {

        base.SkillEvent(skillPacket);
        //보여주기용 좌표 

        _coMove = StartCoroutine(CoMoveSpear());

        _ec.transform.localPosition = _moveDir * 0.5f;
        _ec.transform.localRotation = _rot;
        _ec = null;

    }


    IEnumerator CoMoveSpear(float time = 0.2f)
    {
      
        _isRot = false;

        Vector3 newPos = new Vector3(_moveDir.x * 0.5f, _moveDir.y * 0.5f);
        Quaternion newRot = Util.LookAt2D(_moveDir, Vector2.zero, FacingDirection.UP);

        transform.localPosition = newPos;
        transform.rotation = newRot;

        yield return new WaitForSeconds(time);
        transform.localPosition = Vector3.zero;

        _isRot = true;
        _coMove = null;
   

    }
  
}
