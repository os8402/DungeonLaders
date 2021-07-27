﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{

    private ChasePlayerCam _cam;
    public ChasePlayerCam Cam { get { return _cam; } }

    [SerializeField]
    private bool _moveKeyPressed = false;
    private bool _mouseKeyPressed = false;

    //죽었을 때 이벤트 날려서 물어봐야 함 [바로 사라지지않기 때문에] 
    public Action<PlayerController> deadTargetEvent = null;

    protected override void Init()
    {
        MaxHp = 1000;

        base.Init();
   
        Transform camera = Camera.main.transform;
        _cam = camera.GetComponent<ChasePlayerCam>();
        _cam.Init();

        Managers.Input.keyInputEvent -= UpdateInput;
        Managers.Input.keyInputEvent += UpdateInput;

    }



    void Start()
    {
        Init();
    }
  
    protected override void UpdateController()
    {
        _moveKeyPressed = Managers.Input.PressMoveKey();

        base.UpdateController();
    }

    protected override void UpdateRotation()
    {
        Quaternion q = Util.RotateDir2D(transform.position, _cam.MousePos, true);

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
    protected override void UpdateIdle()
    {

        if (_moveKeyPressed)
        {
            CL_STATE = ControllerState.Move;
            return;
        }

        CL_STATE = ControllerState.Idle;
    }

    protected override void MoveToNextPos()
    {

        if (_moveKeyPressed == false)
        {
            CL_STATE = ControllerState.Idle;
            return;
        }

        Vector3Int destPos = Pos;
     
        destPos += new Vector3Int((int)Managers.Input.H, (int)Managers.Input.V, 0);
    
        if (Managers.Map.CanGo(destPos) && Managers.Object.Find(destPos) == null)
        {      
               Pos = destPos;                            
        }

    }

    void UpdateInput()
    {
 
        if (_coSkill == null && Managers.Input.Mouse_Left)
        {
            // 스킬 공격 
            _coSkill = StartCoroutine("CoSkillAttack", 0.2f);
        }
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
