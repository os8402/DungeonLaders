using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : CreatureController
{
    private Transform _target; 
    public Transform Target { get { return _target; } }
   
    void Start()
    {
        Init(); 
    }

    protected override void Init()
    {
        base.Init();
        _target = FindObjectOfType<PlayerController>().transform; 

    }

    void Update()
    {
        UpdateIdle(); 
        UpdateRotation();
    }

    protected override void UpdateRotation()
    {
        if (_target == null)
            return;

        Quaternion q = Util.RotateDir2D(_target.position, transform.position , true);

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

    protected override void UpdateMoving()
    {
        //정찰도 넣어줘야 함
        if (_target == null)
            return;

        //Managers.Map.FindPath(transform.position, _target.position);
    }

    protected override void UpdateIdle()
    {

    }

    protected override void MoveToNextPos()
    {
       
    }
}
