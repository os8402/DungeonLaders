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


    public int Hp
    {
        get { return Stat.Hp; }
        set { Stat.Hp = value; UpdateHpBar(); }    
    }


    protected Transform _hand;
    public EquipWeapon MyWeapon { get; set;  }
    public Weapons WEAPON_TYPES { get; protected set; } = Weapons.Empty;

    protected Action<S_Skill> _skillEvent = null;
    public Vector3 TargetPos { get; set; }

    void Start()
    {
        Init();
    }
    public override DirState Dir
    {
        get { return PosInfo.Dir; }
        set
        {
            base.Dir = value;
       
            if (_hand == null)
                return;

            // TODO : 나중에 json에서 파싱된 값을 가져와야 함
            Vector2 hand = Vector2.zero;

            if(MyWeapon.GetType() == typeof(Sword) || MyWeapon.GetType() == typeof(Spear))
                hand = new Vector2(-0.2f, -0.3f);
            else
                hand = new Vector2(-0.2f, 0f);

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

    }

    protected void AddHpBar()
    {
        GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
        go.transform.localPosition = new Vector3(0, 0.2f, 0);
        go.name = "HpBar";
        _hpBar = go.GetComponent<HpBar>();
        UpdateHpBar();
    }

    void UpdateHpBar()
    {
        if (_hpBar == null)
            return;

        float ratio = 0.0f;
        if (Stat.MaxHp > 0)      
            ratio = ((float)Hp / Stat.MaxHp);
            
        _hpBar.SetHpBar(ratio);
 
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
        Data.Weapon weapon  = null;
        int id = weaponInfo.WeaponId;
        if (Managers.Data.WeaponDict.TryGetValue(id, out weapon) == false)
            return;

        if (weapon.weaponType == Weapons.Empty)
            return;

        WEAPON_TYPES = weapon.weaponType;

        _hand = Util.FindChild<Transform>(gameObject, "Weapon_Hand", false);
        string name = weapon.weaponType.ToString();

        int typeId = id % 100; 
       
        GameObject go = Managers.Resource.Instantiate($"Weapon/{name}_{typeId.ToString("000")}");
        go.transform.parent = _hand;
        go.transform.localPosition = Vector3.zero;

        MyWeapon = go.GetComponent<EquipWeapon>();
        MyWeapon.Id = typeId;


        _skillEvent -= MyWeapon.SkillEvent;
        _skillEvent += MyWeapon.SkillEvent;

   
    }

    protected override void UpdateRotation() 
    {

        if (_q.z > Quaternion.identity.z) // 오른쪽       
            Dir = DirState.Right;

        else if (_q.z < Quaternion.identity.z)// 왼쪽    
            Dir = DirState.Left;

        else
            return;

    }

    protected override void MoveToNextPos() { }

    protected override void UpdateIdle() { }

    public virtual void UseSkill(S_Skill skillPacket)
    {
        float targetX = skillPacket.TargetInfo.TargetX;
        float targetY = skillPacket.TargetInfo.TargetY;

        TargetPos = new Vector3(targetX, targetY);
        Dir = skillPacket.TargetInfo.Dir;

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

        _hpBar.gameObject.SetActive(false);
        _hand.gameObject.SetActive(false);

    }

    protected virtual void CheckUpdatedFlag() { }



}
