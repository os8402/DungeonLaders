using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : BaseController
{
 

    HpBar _hpBar;
    public override StatInfo Stat
    {
        get { return base.Stat; }
        set { base.Stat = value; UpdateHpBar(); }
    }


    public virtual int Hp
    {
        get { return Stat.Hp; }
        set { Stat.Hp = value; UpdateHpBar(); }
    }
    public virtual int Mp
    {
        get { return Stat.Mp; }
        set { Stat.Mp = value; }
   
    }
    public virtual int Exp
    {
        get { return Stat.CurExp; }
        set { Stat.CurExp = value; }
   
    }


    protected Transform _hand;
    public Vector2 HandPos { get; set; }
    public EquipWeapon MyWeapon { get; set;  }
    public WeaponType WEAPON_TYPES { get; protected set; } = WeaponType.None;

    protected Action<S_Skill> _skillEvent = null;

    void Start()
    {
        Init();
    }

    public override DirState Dir
    {
        get { return PosInfo.Target.Dir; }
        set
        {
            base.Dir = value;
       
            if (_hand == null)
                return;

            Vector2 hand = HandPos;
            hand.x = (Dir == DirState.Right ? hand.x * -1 : hand.x);
            _hand.localPosition = hand;

        }
    }

    protected override void Init()
    {
        base.Init();
        GameObject eff = Managers.Resource.Instantiate("Effect/Common/Resurrect_Eff");
        eff.transform.position = new Vector3(transform.position.x + 0.25f, transform.position.y) ;
        AddHpBar();

        if (Hp == 0)
        {
            UpdateAnimation();
            OnDead();
        }

    }

    protected void AddHpBar()
    {
        GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
        go.transform.localPosition = new Vector3(0, 0.2f, 0);
        go.name = "HpBar";
        _hpBar = go.GetComponent<HpBar>();
        UpdateHpBar();
    }

    public void UpdateHpBar()
    {
        if (_hpBar == null)
            return;

        float ratio = 0.0f;
        if (Stat.MaxHp > 0)      
            ratio = ((float)Hp / Stat.MaxHp);
            
        _hpBar.SetHpBar(ratio);
 
    }

    public void LevelUp()
    {
        Exp = 0;

        GameObject eff = Managers.Resource.Instantiate("Effect/Common/LevelUp_Eff");
        eff.transform.position = new Vector3(transform.position.x + 0.25f, transform.position.y);

    }
    public void ConvertColorHit()
    {
        if (_spriteRenderer == null)
            return;

        if (_spriteRenderer.color == Color.white)
        {
            _spriteRenderer.color = Color.red;
            Invoke("ConvertColorHit", 0.2f);
        }

        else
        {
            CancelInvoke();
            _spriteRenderer.color = Color.white;
        }

    }

    public void CreateWeapon(WeaponInfo weaponInfo)
    {
        ItemData itemData  = null;
        int id = weaponInfo.WeaponId;
        if (Managers.Data.ItemDict.TryGetValue(id, out itemData) == false)
            return;

        WeaponData weaponData = (WeaponData)itemData;

        if (weaponData.weaponType == WeaponType.None)
            return;

        WEAPON_TYPES = weaponData.weaponType;

        _hand = Util.FindChild<Transform>(gameObject, "Weapon_Hand", false);
        string name = weaponData.weaponType.ToString();

        int typeId = id % 100;

     

        GameObject go = Managers.Resource.Instantiate($"Weapon/{name}_{typeId.ToString("000")}");
        go.transform.parent = _hand;
        go.transform.localPosition = Vector3.zero;

        MyWeapon = go.GetComponent<EquipWeapon>();
        MyWeapon.Id = typeId;


        MyWeapon.WeaponDamage = weaponData.damage;
        MyWeapon.CoolDown = weaponData.cooldown;
        MyWeapon.AttackRange = weaponData.attackRange;
        MyWeapon.EffPos = new Vector3(weaponData.effPosX, weaponData.effPosY);
        HandPos = new Vector2(weaponData.handPosX, weaponData.handPosY);


        _skillEvent -= MyWeapon.SkillEvent;
        _skillEvent += MyWeapon.SkillEvent;

   
    }
    protected override void UpdateRotation() { }

    protected override void MoveToNextPos() { }

    protected override void UpdateIdle() { }

    public virtual void UseSkill(S_Skill skillPacket)
    {

        Target = skillPacket.TargetInfo;
        _skillEvent?.Invoke(skillPacket);

 
       
    }

    public virtual void OnDamaged()
    {
        ConvertColorHit();
    }

    public virtual void OnDead()
    {
        CL_STATE = ControllerState.Dead;

        if (Managers.Object.MyPlayer == this)
            Managers.Input.Clear();

        transform.position = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);

        if (_hpBar == null)
            return;
        if (_hand == null)
            return; 
        _hpBar.gameObject.SetActive(false);
        _hand.gameObject.SetActive(false);

    }




  
}
