using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : BaseController
{


    public int Hp { get; protected set; } = 100;

    public int TeamId { get; set; }

    protected Transform _hand;
    protected BaseWeapon _myWeapon;
    public Weapons WEAPONS { get; protected set; } = Weapons.Empty;

    protected Coroutine _coSkill = null;
    protected Action _skillEvent = null;

    private Coroutine _coDead = null; 
    

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
            if (_hand == null)
                return;

            Vector2 hand = new Vector2(-0.2f, -0.3f);
            hand.x = (_dir == 1 ? hand.x * -1 : hand.x);
            _hand.localPosition = hand;

        }
    }
    protected override void Init()
    {
        base.Init();
        Managers.Object.Add(gameObject);
        GameObject go = Managers.Resource.Instantiate("Effect/Common/Resurrect_Eff");
        go.transform.position = new Vector3(transform.position.x + 0.25f, transform.position.y) ;
    }


    public void CreateWeapon(Weapons weapon, int idx)
    {
        if (weapon == Weapons.Empty)
            return;

        WEAPONS = weapon;

        _hand = Util.FindChild<Transform>(gameObject, "Weapon_Hand", false);

        string name = weapon.ToString();

       GameObject go =Managers.Resource.Instantiate($"Weapon/{name}_{idx.ToString("000")}");
        go.transform.parent = _hand;
        go.transform.localPosition = Vector3.zero;

        _myWeapon = go.GetComponent<BaseWeapon>();
        _skillEvent -= _myWeapon.SkillEvent;
        _skillEvent += _myWeapon.SkillEvent;

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

        _coDead = StartCoroutine(CoCreatureDead(3 , attacker));



    }

    IEnumerator CoCreatureDead(float time , GameObject attacker)
    {
        _hand.gameObject.SetActive(false);

        //TODO : 죽엇다고 이벤트 전달
     
        yield return new WaitForSeconds(time);

        Managers.Object.Remove(gameObject);
        CreatureController cc = gameObject.GetComponent<CreatureController>();

        int nameIndex = gameObject.name.LastIndexOf('_');
        string myName = gameObject.name.Substring(0, nameIndex);

        //부.활
        Managers.Object.CreateCreature(myName, id, TeamId, WEAPONS);

    }



}
