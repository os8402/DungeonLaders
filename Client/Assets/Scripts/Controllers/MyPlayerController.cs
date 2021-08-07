using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    private ChasePlayerCam _cam;
    public ChasePlayerCam Cam { get { return _cam; } }


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
    protected override void MoveToNextPos()
    {
        if (_moveKeyPressed == false)
        {
            CL_STATE = ControllerState.Idle;
            CheckUpdatedFlag();
            return;
        }

        Vector3Int destPos = CellPos;

        destPos += new Vector3Int((int)Managers.Input.H, (int)Managers.Input.V, 0);

        if (Managers.Map.CanGo(destPos) && Managers.Object.Find(destPos) == null)
        {
            CellPos = destPos;
        }
        CheckUpdatedFlag();

    }
    protected void CheckUpdatedFlag()
    {
        if (_updated)
        {
            Debug.Log("패킷 전송");
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
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
    protected override void UpdateIdle()
    {

        if (_moveKeyPressed)
        {
            CL_STATE = ControllerState.Moving;
            return;
        }

        CL_STATE = ControllerState.Idle;
    }

    protected override void UpdateController()
    {
        TargetPos = _cam.MousePos;
        _moveKeyPressed = Managers.Input.PressMoveKey();

        base.UpdateController();
    }

 

}
