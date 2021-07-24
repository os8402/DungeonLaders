using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    private PlayerController _target; 
    public PlayerController Target { get { return _target; } }

   
    void Start()
    {
        Init(); 
    }

    protected override void Init()
    {
        base.Init();
        _target = FindObjectOfType<PlayerController>();

        CL_STATE = ControllerState.Move;
        Speed = 5f;
    }



    protected override void UpdateRotation()
    {
        if (_target == null)
            return;

        Quaternion q = Util.RotateDir2D(_target.transform.position, transform.position , true);

        if (q.z > Quaternion.identity.z) // 오른쪽
        {
            Dir = -1;
        }
        else if (q.z < Quaternion.identity.z)// 왼쪽
        {
            Dir = 1;
        }

        else return;
    }

  

    protected override void UpdateIdle()
    {

    }

    int _chaseCellDist = 100;

    protected override void MoveToNextPos()
    {
        ////정찰도 넣어줘야 함
        //if (_target == null)
        //    return;

        //List<Vector3Int> path = Managers.Map.FindPath(Pos, _target.Pos);
        //Vector3Int dir = _target.Pos - Pos;
        //int dist = Managers.Map.CellDistFromZero(dir.x, dir.y);

        //if (dist == 1 || dist > _chaseCellDist)
        //{
        //    _target = null;
        //    CL_STATE = ControllerState.Idle;
        //    return;
        //}


        //if (path.Count < 2 || path.Count > _chaseCellDist)
        //{
        //    _target = null;
        //    CL_STATE = ControllerState.Idle;
        //    return;
        //}

        //if (Managers.Map.CanGo(path[1]))
        //{
        //    if (Managers.Object.FindCreature(path[1]) == null)
        //    {
        //        Pos = path[1] - Pos;
        //    }
        //}

       // Pos = path[1] - Pos;
    }
}
