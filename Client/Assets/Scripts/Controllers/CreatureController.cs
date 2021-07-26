using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : BaseController
{

    public int Hp { get; private set; } = 100;

    protected Transform _hand;
    protected BaseWeapon _myWeapon;
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

        _myWeapon = go.GetComponent<BaseWeapon>();
        _skillEvent -= _myWeapon.SkillEvent;
        _skillEvent += _myWeapon.SkillEvent;

    }

    protected override void Init()
    {
        base.Init();
       
    }


    protected override void MoveToNextPos() { }

    protected override void UpdateIdle() { }

    protected override void UpdateRotation() { }

    public virtual void OnDamaged(GameObject attacker, int damage)
    {
        //나중에 서버로 옮길건데 멀티스레드에선 갑자기 없어질 수도 잇어서 return
        if (attacker == null)
            return;

        if (CL_STATE == ControllerState.Death)
            return;

        damage = Mathf.Max(0, damage);
        Hp = Mathf.Max(0, Hp - damage);

        if(Hp <= 0)
        {
           
            OnDead(attacker);
            return;
        }


        ConvertColorHit();

    }
    
    void ConvertColorHit()
    {
        if(_spriteRenderer.color == Color.white)
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

    public virtual void OnDead(GameObject attacker)
    {
        Debug.Log($"{attacker.name} -> {gameObject.name} Kill!");
        CL_STATE = ControllerState.Death;

        //시체가 남는걸로..  
        Managers.Resource.Destroy(_hand.gameObject);
      
    }

}
