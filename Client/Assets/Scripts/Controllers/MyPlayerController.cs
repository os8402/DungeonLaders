using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{

    bool _moveKeyPressed = false;
    bool _mouseKeyPressed = false;

    private ChasePlayerCam _cam;
    public ChasePlayerCam Cam { get { return _cam; } }


    protected override void Init()
    {

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
        destPos += Vector3Int.FloorToInt(Managers.Input.GetAxis);

        if (Managers.Map.CanGo(destPos) && Managers.Object.FindCreature(destPos) == null)
        {
            CellPos = destPos;
        }
        CheckUpdatedFlag();

    }


    void UpdateInput()
    {
        if (_coSkillCoolTime == null && Managers.Input.Mouse_Left)
        {
            SendSkill();
        }
    }

    void SendSkill()
    {

        Debug.Log("Skill!");

        C_Skill skill = new C_Skill()
        {
            AttackPos = new AttackPos(),
            TargetInfo = new TargetInfo()
        };

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);

        Vector3Int attackPos = Managers.Map.CurrentGrid.WorldToCell(mousePos);

        //본인을 직접 공격하면 그냥 return
        if (attackPos == CellPos)
            return;

        skill.AttackPos.AttkPosX = attackPos.x;
        skill.AttackPos.AttkPosY = attackPos.y;

        skill.TargetInfo.TargetX = TargetPos.x;
        skill.TargetInfo.TargetY = TargetPos.y;
        skill.TargetInfo.Dir = Dir;


        Managers.Network.Send(skill);

        _coSkillCoolTime = StartCoroutine("CoInputCoolTime", 0.2f);


    }

    Coroutine _coSkillCoolTime;
    IEnumerator CoInputCoolTime(float time)
    {
        yield return new WaitForSeconds(time);
        _coSkillCoolTime = null;
    }
    protected override void UpdateController()
    {
        TargetPos = _cam.MousePos;
        _moveKeyPressed = Managers.Input.PressMoveKey();

        base.UpdateController();
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
