using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{


    protected override void Init()
    { 
        base.Init();

        MonsterData monsterData = null;
        Managers.Data.MonsterDict.TryGetValue(1, out monsterData);

        if (monsterData == null)
            return;

        JobName = monsterData.name;

    }


    public override void UseSkill(S_Skill skillPacket)
    {

        CL_STATE = ControllerState.Skill;
        base.UseSkill(skillPacket);
    }

    protected override void UpdateSkill()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        float dist = moveDir.magnitude;

        if (dist < Speed * Time.deltaTime)     
            transform.position = destPos;
     
        else        
            transform.position += moveDir.normalized * Speed * Time.deltaTime;
        
    }

    //IEnumerator CoSearch()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(1);

    //        if (_player != null)
    //            continue;

    //        _player = Managers.Object.Find((go) =>
    //        {
    //            if (go == null)
    //                return false; 

    //            PlayerController pc = go.GetComponent<PlayerController>();
    //            if (pc == null)
    //                return false;

    //            if (pc.Hp <= 0)
    //                return false;

    //            Vector3Int dir = (pc.CellPos - CellPos);
    //            if (dir.magnitude > _searchRange)
    //                return false;

    //            return true;
    //        });

    //        if (_player != null)
    //            CL_STATE = ControllerState.Moving;

    //    }
    //}


}
