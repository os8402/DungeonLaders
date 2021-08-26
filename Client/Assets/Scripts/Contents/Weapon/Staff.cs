using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class Staff : EquipWeapon
{


    protected override void UpdateRotation()
    {

        Quaternion prevQ = _q;

        _q = Util.LookAt2D(transform.position, _targetPos, FacingDirection.UP);
        transform.rotation = Quaternion.Slerp(prevQ, _q, 1f);
    }


    public override void SkillEvent(S_Skill skillPacket)
    {

        base.SkillEvent(skillPacket);
        //보여주기용 좌표 

        _ec.transform.localPosition = _moveDir * 0.5f;
        _ec.transform.localRotation = _rot;
        _ec = null;

    }


}
