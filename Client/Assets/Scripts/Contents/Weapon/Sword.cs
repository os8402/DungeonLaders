using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : BaseWeapon
{
    //TODO : 검은 공격할 때 방향에 따라 Sorting Order도 적용
    private int _swordDir = 0;
    
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

    void Update()
    {
        UpdateRotation();
    }

    protected override void UpdateRotation()
    {
        if (_owner.Dir == -1)
        {
            //무기의 위치 , 마우스 위치
            switch (_swordDir)
            {
                //Up
                case 0:
                    _q = Util.LookAt2D(transform.position, _owner.Cam.MousePos);
                    break;
                case 1:
                    _q = Util.LookAt2D(_owner.Cam.MousePos, transform.position);
                    break;
            }
        }
        else
        {
            switch (_swordDir)
            {
                case 0:

                    _q = Util.LookAt2D(_owner.Cam.MousePos, transform.position);
                    break;
                case 1:
                    _q = Util.LookAt2D(transform.position, _owner.Cam.MousePos);
                    break;
            }
        }

        transform.rotation = _q;
    }

    public override void SkillEvent()
    {
        SwordDir = (1 - SwordDir);  // -1 연산 
    }


}
