using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : BaseController
{

    protected int Hp { get; private set; } = 100;

    protected Transform _hand;
    protected BaseWeapon myWeapon;
    public Weapons WEAPONS { get; protected set; } = Weapons.Empty;

    protected Coroutine _coSkill = null;
    protected Action _skillEvent = null;

    void Start()
    {
        Init();
    }
    public override int Dir
    {
        get { return _dir; }
        set
        {
            base.Dir = value; 
            // TODO : 나중에 json에서 파싱된 값을 가져와야 함
            Vector2 hand = new Vector2(-0.2f, -0.3f);
            hand.x = (_dir == 1 ? hand.x * -1 : hand.x);
            _hand.localPosition = hand;

        }
    }

    public void CreateWeapon(Weapons weapon, int idx)
    {
        if (weapon == Weapons.Empty)
            return;

        _hand = Util.FindChild<Transform>(gameObject, "Weapon_Hand", true);

        string name = weapon.ToString();

       GameObject go =Managers.Resource.Instantiate($"Weapon/{name}_{idx.ToString("000")}");
        go.transform.parent = _hand;
        go.transform.localPosition = Vector3.zero;

        myWeapon = go.GetComponent<BaseWeapon>();
        _skillEvent -= myWeapon.SkillEvent;
        _skillEvent += myWeapon.SkillEvent;

    }

    protected override void Init()
    {
        base.Init();
       
    }


    protected override void MoveToNextPos() { }

    protected override void UpdateIdle() { }

    protected override void UpdateRotation() { }


}
