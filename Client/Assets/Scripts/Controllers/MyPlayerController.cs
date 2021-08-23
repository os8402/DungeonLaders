using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{

    bool _moveKeyPressed = false;

    private ChasePlayerCam _cam;
    public ChasePlayerCam Cam { get { return _cam; } }

    public string JobName { get; set; }
    public override int Hp
    {
        get { return Stat.Hp; }
        set
        {
            base.Hp = value;
            statusUI.RefreshUI();
            statUI.RefreshUI();
        }
    }
    public override int Mp
    {
        get { return Stat.Mp; }
        set
        {
            base.Mp = value;
            statusUI.RefreshUI();
            statUI.RefreshUI();
        }
    }
    public override int Exp
    {
        get { return Stat.CurExp; }
        set
        {
            base.Exp = value;
            statusUI.RefreshUI();
            statUI.RefreshUI();
        }
    }

    public int WeaponDamage { get; private set; }
    public int ArmorDefence { get; private set; }
    public int ArmorHp { get; private set; }
    public float ArmorSpeed { get; private set; }

    public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
    public override int TotalDefence { get { return ArmorDefence; } }
    public override int TotalHp { get { return Stat.MaxHp + ArmorHp; } }
    public override float TotalSpeed { get { return Stat.Speed + ArmorSpeed; } }



    UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
    UI_Inventory invenUI;
    UI_Status statusUI;
    UI_Stat statUI;
    UI_Passive passiveUI;
    UI_Coin coinUI;

    protected override void Init()
    {
        base.Init();


        invenUI = gameSceneUI.InvenUI;
        statUI = gameSceneUI.StatUI;
        statusUI = gameSceneUI.StatusUI;
        coinUI = gameSceneUI.CoinUI;
        passiveUI = gameSceneUI.PassiveUI;


        statusUI.gameObject.SetActive(true);
        coinUI.gameObject.SetActive(true);
        passiveUI.gameObject.SetActive(true);

        RefreshCalcStat();

        Transform camera = Camera.main.transform;
        _cam = camera.GetComponent<ChasePlayerCam>();
        _cam.Init();

        Managers.Input.keyInputEvent -= UpdateInput;
        Managers.Input.keyInputEvent += UpdateInput;

    }
    public override void CreateWeapon(int weaponId)
    {
        base.CreateWeapon(weaponId);
        RefreshCalcStat();
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
            bool active = invenUI.gameObject.activeSelf;
            invenUI.gameObject.SetActive(!active);

            if (active == false)
                invenUI.RefreshUI();
        }

        if (Managers.Input.C)
        {
          
            bool active = statUI.gameObject.activeSelf;
            statUI.gameObject.SetActive(!active);

            if (active == false)
                statUI.RefreshUI();
        }

    }


    void UpdateInput()
    {
        GetUIKey();

        //UI 창 띄울 땐 공격 x

        if(invenUI.gameObject.activeSelf || statUI.gameObject.activeSelf)     
            return;
        

        if (MyWeapon != null && _coSkillCoolTime == null && Managers.Input.Mouse_Left)
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

        _coSkillCoolTime = StartCoroutine("CoInputCoolTime", MyWeapon.CoolDown);


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

    public override void OnDead()
    {
        base.OnDead();
        statUI.gameObject.SetActive(false);
        invenUI.gameObject.SetActive(false);
    }

    public void RefreshCalcStat()
    {
        WeaponDamage = 0;
        ArmorDefence = 0;
        ArmorHp = 0;
        ArmorSpeed = 0;

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
                    ArmorHp += ((Armor)item).Hp;
                    ArmorSpeed += ((Armor)item).Speed;

                    break;
            }
        }

        UpdateHpBar();
        
        if (statusUI != null)
         statusUI.RefreshUI();
    }

}
