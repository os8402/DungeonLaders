using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{

    bool _moveKeyPressed = false;

    private ChasePlayerCam _cam;
    public ChasePlayerCam Cam { get { return _cam; } }

    public int WeaponDamage { get; set; }
    public int ArmorDefence { get; set; }

    protected override void Init()
    {
        base.Init();

        RefreshCalcStat();

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

    void GetUIKey()
    {
        if (Managers.Input.I)
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Inventory invenUI = gameSceneUI.InvenUI;

            bool active = invenUI.gameObject.activeSelf;
            invenUI.gameObject.SetActive(!active);

            if (active == false)
                invenUI.RefreshUI();
        }

        if(Managers.Input.C)
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Stat statUI = gameSceneUI.StatUI;

            bool active = statUI.gameObject.activeSelf;
            statUI.gameObject.SetActive(!active);

            if(active == false)
                statUI.RefreshUI();
        }
    }


    void UpdateInput()
    {
        GetUIKey();

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
        skill.TargetInfo = Target;


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
    protected override void UpdateRotation()
    {
        Vector3 targetPos = new Vector3(Target.TargetX, Target.TargetY);
        Quaternion _q = Util.RotatePlayer2D(transform.position, targetPos);

        if (_q.z > Quaternion.identity.z) // 오른쪽       
            Dir = DirState.Right;

        else if (_q.z < Quaternion.identity.z)// 왼쪽    
            Dir = DirState.Left;

        else
            return;

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

    public void RefreshCalcStat()
    {
        WeaponDamage = 0;
        ArmorDefence = 0;

        foreach (Item item in Managers.Inven.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    WeaponDamage += ((Weapon)item).Damage;
                    break;
                case ItemType.Armor:
                    ArmorDefence += ((Armor)item).Defence;
                    break;
            }
        }
    }

}
