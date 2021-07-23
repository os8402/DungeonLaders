using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Sword : BaseWeapon
{
    //TODO : 검은 공격할 때 방향에 따라 Sorting Order도 적용
    private int _swordDir = 0;
    public Transform temp_target;
    public int SwordDir
    {
        get { return _swordDir; }
        set
        {
            _swordDir = value;
            int ownerOrder = _owner.SpriteRenderer.sortingOrder;
            int myOrder = (_swordDir == 1 ? ownerOrder + 5 : ownerOrder - 5);
            _spriteRenderer.sortingOrder = myOrder;
        }
    }

    void Awake()
    {
        Init();
   
       
    }
    protected override void Init()
    {
        base.Init();
        _attackRange = 2.0f;
    }

    protected override void UpdateRotation()
    {
        //if (temp_target == null)
        //    temp_target = GameObject.Find("1").transform;

        //Vector3 targetPos = temp_target.position.normalized;

        //float dot = Vector3.Dot(targetPos, transform.up);

        //Debug.Log($"{gameObject.name}  :  {dot}");



        if (_owner.Dir == -1)
        {
            //무기의 위치 , 마우스 위치
            switch (_swordDir)
            {
                //Up
                case 0:
                    _q = Util.LookAt2D(transform.position, _targetPos);
                    break;
                case 1:
                    _q = Util.LookAt2D(_targetPos, transform.position);
                    break;
            }
        }
        else
        {
            switch (_swordDir)
            {
                case 0:

                    _q = Util.LookAt2D(_targetPos, transform.position);
                    break;
                case 1:
                    _q = Util.LookAt2D(transform.position, _targetPos);
                    break;
            }
        }

        transform.rotation = _q;
    }

    public override void SkillEvent()
    {
        SwordDir = (1 - SwordDir);  // -1 연산 

        GameObject go = Managers.Resource.Instantiate("Effect/Sword/Sword_Eff_001");
        EffectController ec = go.GetComponent<EffectController>();

        Vector3 moveDir = _targetPos - transform.position;
        Quaternion rot;

        rot = Util.LookAt2D(_targetPos, transform.position, FacingDirection.LEFT);

        ec.transform.parent = _owner.transform;
        ec.transform.localPosition = moveDir.normalized * 1.2f;
        ec.Pos = Vector3Int.RoundToInt(ec.transform.position);
        ec.Dir = _owner.Dir;
        ec.Owner = _owner;


        ec.transform.localRotation = rot;
      //  Debug.Log(ec.Pos);

 

    }


}
