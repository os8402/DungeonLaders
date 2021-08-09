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

    void UpdateInput()
    {

        if (_coSkillCoolTime == null && Managers.Input.Mouse_Left)
        {
            Debug.Log("Skill!");

            C_Skill skill = new C_Skill()
            {
                TargetInfo = new TargetInfo()
            };
     
            skill.TargetInfo.TargetPosX = TargetPos.x;
            skill.TargetInfo.TargetPosY = TargetPos.y;

            Managers.Network.Send(skill);

            _coSkillCoolTime = StartCoroutine("CoInputCoolTime",0.2f);

        }
    }

    Coroutine _coSkillCoolTime; 
    IEnumerator CoInputCoolTime(float time)
    {
        yield return new WaitForSeconds(time);
        _coSkillCoolTime = null;
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

    protected override void CheckUpdatedFlag()
    {
        if (_updated)
        {
 
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }


}
